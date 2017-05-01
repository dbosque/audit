
namespace dBosque.EntityFramework.Audit
{

    /// <summary>
    /// Settings to control the behaviour of the auditing
    /// </summary>
    public class AuditSettings
    {
        /// <summary>
        /// The type of <seealso cref="TableTracking"/> to use.
        /// </summary>
        public TableTracking Tracking { get; set; } = TableTracking.Default;

        /// <summary>
        /// If set tot true, all values will be compared to the values in the database to determine if they have been changed or not. Otherwise 
        /// changes are detected via the inmemory values.
        /// </summary>
        public bool UseDatabaseValueCompare { get; set; } = false;

        /// <summary>
        /// If set to true, all changed values will be audited
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}
