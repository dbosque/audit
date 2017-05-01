
namespace dBosque.EntityFramework.Audit
{
    /// <summary>
    /// TableTracking to implement
    /// </summary>
    public enum TableTracking
    {
        /// <summary>
        /// Use the default behaviour, track only fields en classes that are explicitly marked Auditable, ignoring
        /// fields and classes that are marked NotAuditable
        /// </summary>
        Default,
        /// <summary>
        /// Track all classes and properties, ignoring the attributes
        /// </summary>
        All,
        /// <summary>
        /// Track all classes and properties, except the once that are explicitly marked NotAuditable.
        /// </summary>
        AllExcept
    }
}
