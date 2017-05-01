using dBosque.EntityFramework.Audit.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace dBosque.EntityFramework.Audit
{
 
    /// <summary>
    /// Cached version of the <seealso cref="DbContextAuditorImplementation{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DbContextAuditorCache<T> : DbContextAuditorImplementation<T> where T : IAuditLog
    {
        /// <summary>
        /// The current context configuration
        /// </summary>
        private readonly AuditConfiguration _contextConfiguration;

        internal DbContextAuditorCache(DbContext context, AuditSettings settings)
            : base(context, settings.UseDatabaseValueCompare)
        {
            _contextConfiguration = AuditConfiguration.CreateConfigurationFor(_context, settings.Tracking);
            filter = q => _contextConfiguration.IsAuditable.Contains(ToRealType(q.Entity.GetType()));
        }

        protected override List<string> IncludedPropertiesOfType(DbEntityEntry entity, string name)
        {
            return _contextConfiguration.AuditableProperties[name];
        }
    }


   


  
}