using dBosque.EntityFramework.Audit.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace dBosque.EntityFramework.Audit
{
    /// <summary>
    /// A container class to hold all audit configurations
    /// </summary>
    internal class AuditConfiguration
    {
        public AuditConfiguration()
        {
            IsAuditable = new List<Type>();
            AuditableProperties = new Dictionary<string, List<string>>();
        }
        /// <summary>
        /// All types that are auditable
        /// </summary>
        public List<Type> IsAuditable { get; set; }

        /// <summary>
        /// All properties that are auditable given a specific typename
        /// </summary>
        public Dictionary<string, List<string>> AuditableProperties;

        /// <summary>
        /// All collected configurations
        /// </summary>
        private static Dictionary<string, AuditConfiguration> _configuration = new Dictionary<string, AuditConfiguration>();

        #region Auditconfiguration collection

        /// <summary>
        /// Create a configuration object for the given context and tracking support
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tracking"></param>
        /// <returns></returns>
        public static AuditConfiguration CreateConfigurationFor(DbContext context, TableTracking tracking)
        {
            if (!_configuration.ContainsKey(context.Database.Connection.ConnectionString))
            {
                var auditConfiguration = new AuditConfiguration();
                var workspace = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
                var itemCollection = (ObjectItemCollection)(workspace.GetItemCollection(DataSpace.OSpace));

                context.GetType().GetProperties()
                        .Where(q => q.PropertyType.IsGenericType && q.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                        .Select(p => p.PropertyType.GetGenericArguments()[0])
                        .ToList().ForEach(a => PrepareAuditableProperties(a, itemCollection, auditConfiguration, tracking));

                _configuration[context.Database.Connection.ConnectionString] = auditConfiguration;
            }
            return _configuration[context.Database.Connection.ConnectionString];
        }

        /// <summary>
        /// Update the <seealso cref="AuditConfiguration"/> with all properties from the given entity type.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="eType"></param>
        /// <param name="audit"></param>
        /// <param name="tableTracking"></param>
        private static void PrepareAuditableProperties(Type entityType, ObjectItemCollection eType, AuditConfiguration audit, TableTracking tableTracking)
        {
            // NavigationProperties can not be handled.
            var f = eType.OfType<EntityType>()
                .Single(e => eType.GetClrType(e) == entityType).NavigationProperties
                .Select(a => a.Name);

            List<string> includedProperties = new List<string>();
            if (tableTracking == TableTracking.All)
            {
                includedProperties = entityType.GetProperties()
                    .Where(pi => !f.Contains(pi.Name))
                    .Select(a => a.Name).ToList();
            }
            else if (tableTracking == TableTracking.AllExcept)
            {
                includedProperties = entityType.GetProperties()
                 .Where(p => !p.IsAttr<NotAuditableAttribute>() && !f.Contains(p.Name))
                 .Select(pi => pi.Name).ToList();
            }
            else if (entityType.IsAttr<AuditableAttribute>())
            {
                includedProperties = entityType.GetProperties()
                    .Where(pi => !pi.IsAttr<NotAuditableAttribute>() && !f.Contains(pi.Name))
                    .Select(a => a.Name).ToList();
            }
            else
            {
                includedProperties = entityType.GetProperties()
                    .Where(p => p.IsAttr<AuditableAttribute>() && !p.IsAttr<NotAuditableAttribute>() && !f.Contains(p.Name))
                    .Select(pi => pi.Name).ToList();
            }

            audit.AuditableProperties.Add(entityType.Name, includedProperties);
            if (includedProperties.Any())
                audit.IsAuditable.Add(entityType);
        }
        #endregion
    }
}
