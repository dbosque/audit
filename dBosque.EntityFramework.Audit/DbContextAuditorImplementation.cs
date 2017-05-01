using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dBosque.EntityFramework.Audit
{
    /// <summary>
    /// The actual implementation of the auditor context
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class DbContextAuditorImplementation<T> where T : IAuditLog
    {
        /// <summary>
        /// Key separator in the logging
        /// </summary>
        private const string KeySeparator = "►";

        /// <summary>
        /// The current auditable context
        /// </summary>
        protected readonly DbContext _context;

        /// <summary>
        /// Perform changed value comparison against database or internal values.
        /// </summary>
        private readonly bool _databaseValueCompare;

        /// <summary>
        /// Filter to extract types and properties that are not tracked
        /// </summary>
        protected Func<DbEntityEntry, bool> filter;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="context">The context to wrap</param>
        /// <param name="databaseValueCompare"></param>
        protected DbContextAuditorImplementation(DbContext context, bool databaseValueCompare = false)
        {
            _context = context ?? throw new ArgumentNullException("_context");
            _databaseValueCompare = databaseValueCompare;
        }


        protected abstract List<string> IncludedPropertiesOfType(DbEntityEntry entity, string name);

        protected virtual void SaveLog(IAuditLog log)
        { }

        /// <summary>
        /// Create a new logentry
        /// </summary>
        /// <returns></returns>
        protected virtual IAuditLog CreateNewLogItem()
        {
            var set = _context.Set(typeof(T));
            var auditEntity = set.Create() as IAuditLog;
            set.Add(auditEntity);
            return auditEntity;
        }

        /// <summary>
        /// Saves DbContext changes taking into account Audit
        /// </summary>
        /// <param name="context">The current context</param>
        /// <returns></returns>
        internal int SaveChanges(Func<int> save)
        {
            //// ReSharper disable once SuspiciousTypeConversion.Global
            // Get the current name
            var currentPrincipal = Thread.CurrentPrincipal;
            var user = currentPrincipal != null ? (currentPrincipal.Identity).Name : string.Empty;
            user = string.IsNullOrEmpty(user) ? System.Security.Principal.WindowsIdentity.GetCurrent().Name : string.Empty;
            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    // First collect the newly added items, saven the in a second run to the database.
                    // This way the primary keys are filled in.
                    var added = CollectAdded(_context);
                    ProcessModifiedAndDeleted(_context, user);
                    var result = save();
                    ProcessAdded(_context, added, user);
                    result += save();
                    tran.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                    tran.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Saves DbContext changes taking into account Audit
        /// </summary>
        /// <param name="context">The current context</param>
        /// <returns></returns>
        internal async Task<int> SaveChangesAsync(CancellationToken cancellationToken, Func<CancellationToken, Task<int>> saveAsync)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            // Get the current name
            var currentPrincipal = Thread.CurrentPrincipal;
            var user = currentPrincipal != null ? (currentPrincipal.Identity).Name : string.Empty;
            user = string.IsNullOrEmpty(user) ? System.Security.Principal.WindowsIdentity.GetCurrent().Name : string.Empty;
            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    // First collect the newly added items, saven the in a second run to the database.
                    // This way the primary keys are filled in.
                    var added = CollectAdded(_context);
                    ProcessModifiedAndDeleted(_context, user);
                    var result = await saveAsync(cancellationToken);
                    ProcessAdded(_context, added, user);
                    result += await saveAsync(cancellationToken);
                    tran.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                    tran.Rollback();
                    throw;
                }
            }
        }

        private IEnumerable<DbEntityEntry> CollectAdded(DbContext context)
        {
            return context.ChangeTracker.Entries()
               .Where(e => e.State == EntityState.Added)
               .Where(filter)
               .ToList();
        }
        private void ProcessAdded(DbContext context, IEnumerable<DbEntityEntry> addedEntries, string user)
        {
            addedEntries.ToList().ForEach(e => ApplyAuditLog(context, e, LogOperation.Create, user));
        }

        private void ProcessModifiedAndDeleted(DbContext context, string user)
        {
            var modifiedEntries =
                context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Deleted || e.State == EntityState.Modified)
                    .Where(filter)
                    .ToList();

            modifiedEntries.ForEach(e => ApplyAuditLog(context, e ,user));
        }

        /// <summary>
        /// Register audit information
        /// </summary>        
        /// <param name="context">The current context</param>
        /// <param name="entry">DbContext entry to audit</param>
        private void ApplyAuditLog(DbContext context, DbEntityEntry entry, string user)
        {
            LogOperation operation;
            switch (entry.State)
            {
                case EntityState.Added:
                    operation = LogOperation.Create;
                    break;
                case EntityState.Deleted:
                    operation = LogOperation.Delete;
                    break;
                case EntityState.Modified:
                    operation = LogOperation.Update;
                    break;
                default:
                    operation = LogOperation.Unchanged;
                    break;
            }
            // Chain
            ApplyAuditLog(context, entry, operation, user);
        }

        /// <summary>
        /// Register audit information
        /// </summary>
        /// <param name="context">The current context</param>
        /// <param name="entry">DbContext entry to audit</param>
        /// <param name="logOperation">Audit operation</param>
        private void ApplyAuditLog(DbContext context, DbEntityEntry entry, LogOperation logOperation, string user)
        {
            var entityKey = GetEntityString(context, entry.Entity);
            var entityTypeString = GetEntityType(entry);
            var includedProperties = IncludedPropertiesOfType(entry, entityTypeString);

            if (includedProperties.Any() && entry.State == EntityState.Modified)
            {
                ChangedProperty[] changedProperties;
                if (_databaseValueCompare)
                {
                    var originalValues = context.Entry(entry.Entity).GetDatabaseValues();
                    changedProperties = (from propertyName in originalValues.PropertyNames
                                         let propertyEntry = entry.Property(propertyName)
                                         let currentValue = propertyEntry.CurrentValue
                                         let originalValue = originalValues[propertyName]
                                         where (!Equals(currentValue, originalValue) && includedProperties.Contains(propertyName))
                                         select new ChangedProperty
                                         {
                                             Name = propertyName,
                                             CurrentValue = Convert.ToString(currentValue),
                                             OriginalValue = Convert.ToString(originalValue)
                                         }).ToArray();

                }
                else
                {
                    changedProperties = (from propertyName in includedProperties
                                         let propertyEntry = entry.Property(propertyName)
                                         let currentValue = propertyEntry.CurrentValue
                                         let originalValue = propertyEntry.OriginalValue
                                         where (!Equals(currentValue, originalValue))
                                         select new ChangedProperty
                                         {
                                             Name = propertyName,
                                             CurrentValue = Convert.ToString(currentValue),
                                             OriginalValue = Convert.ToString(originalValue)
                                         }).ToArray();
                }
                if (changedProperties.Any())
                {                                       
                    changedProperties.Select(changedProperty =>
                    {
                        var log = CreateNewLogItem();
                        log.Created = DateTime.Now;                      
                        log.EntityFullName = entityTypeString;
                        log.EntityId = entityKey;
                        log.LogOperation = logOperation.ToString();
                        log.User = user;
                        log.OldValue = changedProperty.OriginalValue;
                        log.NewValue = changedProperty.CurrentValue;
                        log.PropertyName = changedProperty.Name;
                        SaveLog(log);
                        return log;
                    }).ToList();
                }
            }
            else if (includedProperties.Any())
            {

                var values = (from propertyName in includedProperties
                                               let propertyEntry = entry.Property(propertyName)
                                               select new ChangedProperty
                                               {
                                                   Name = propertyName,
                                                   // Take the original value in case we are deleting
                                                   CurrentValue = logOperation == LogOperation.Create? Convert.ToString(propertyEntry.CurrentValue):Convert.ToString(propertyEntry.OriginalValue)
                                               });

                var log = CreateNewLogItem();
                log.Created = DateTime.Now;
                log.Entity = PropertyListToXml(values);
                log.EntityFullName = entityTypeString;
                log.EntityId = entityKey;
                log.LogOperation = logOperation.ToString();
                log.User = user;
                SaveLog(log);
            }
        }


        private string PropertyListToXml(IEnumerable<ChangedProperty> properties)
        {            
            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(ChangedProperty[]));
                serializer.WriteObject(ms, properties.ToArray());
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, Convert.ToInt32(ms.Length));
            }
        }

        /// <summary>
        /// Convert a possible proxytype to its original basetype
        /// </summary>
        /// <param name="type">The type to convert</param>
        /// <returns></returns>
        protected Type ToRealType(Type type)
        {
            if (type.Namespace == "System.Data.Entity.DynamicProxies")
            {
                return type.BaseType;
            }
            return type;
        }

        /// <summary>
        /// Get the underlying entity type
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private string GetEntityType(DbEntityEntry entry)
        {
            return ToRealType(entry.Entity.GetType()).Name;
        }

        /// <summary>
        /// Get the combined key of the entity
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static string GetEntityString<Tkey>(IObjectContextAdapter context, Tkey entity) where Tkey : class
        {
            var entityKey = context.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out ObjectStateEntry ose) ? ose.EntityKey : null;
            if (entityKey == null)
            {
                throw new ArgumentNullException("entityKey");
            }

            var result = new StringBuilder();
            if (entityKey.EntityKeyValues.Count() > 1)
            {
                foreach (var entry in entityKey.EntityKeyValues)
                {
                    result.Append(string.Format("{0}={1}{2}", entry.Key, entry.Value, KeySeparator));
                }
                // Remove the last keyseparator
                result.Remove(result.Length - 1, 1);
            }
            else
            {
                var entry = entityKey.EntityKeyValues.First();
                result.Append(string.Format("{0}", entry.Value));
            }
            return result.ToString();
        }
    }
}