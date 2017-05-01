using System.Data.Entity;

namespace dBosque.EntityFramework.Audit
{
    /// <summary>
    /// The interface to be implemented by the entity that will store the auditlog
    /// </summary>
    public interface IAuditLog
    {        
        /// <summary>
        /// The time the entry is created
        /// </summary>
         System.DateTime Created { get; set; }

        /// <summary>
        /// The full name of the entity (table)
        /// </summary>
         string EntityFullName { get; set; }

        /// <summary>
        /// A json representation of the changed object (in delete and create statements)
        /// </summary>
         string Entity { get; set; }

        /// <summary>
        /// The database key of the changed entity
        /// </summary>
         string EntityId { get; set; }

        /// <summary>
        /// The user responsible for the changed values
        /// </summary>
         string User { get; set; }

        /// <summary>
        /// The previous value
        /// </summary>
         string OldValue { get; set; }

        /// <summary>
        /// The new value
        /// </summary>
         string NewValue { get; set; }

        /// <summary>
        /// The property / column that changed.
        /// </summary>
         string PropertyName { get; set; }

        /// <summary>
        /// The operation to took place (delete/update/create)
        /// </summary>
         string LogOperation { get; set; }
    }
}