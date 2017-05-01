namespace dBosque.EntityFramework.Audit
{
    /// <summary>
    ///  The operation to log
    /// </summary>
    internal enum LogOperation
    {
        /// <summary>
        /// Entity was created
        /// </summary>
        Create,
        /// <summary>
        /// Entity value was updated
        /// </summary>
        Update,
        /// <summary>
        /// Entity was deleted
        /// </summary>
        Delete,
        /// <summary>
        /// Entity is unchanged
        /// </summary>
        Unchanged
    }
}