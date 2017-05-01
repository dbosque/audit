using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace dBosque.EntityFramework.Audit
{
    /// <summary>
    /// Container class to keep track of values of a property
    /// </summary>
    [DataContract(Name ="Value", Namespace = "AuditLog")]
    internal class ChangedProperty
    {
        /// <summary>
        /// Name of the changed property
        /// </summary>
        [DataMember(Name ="Name")]
        public string Name;
        /// <summary>
        /// The new value
        /// </summary>
        [DataMember(Name = "Value")]
        public string CurrentValue;        

        /// <summary>
        /// The old value
        /// </summary>
        public string OriginalValue;
    }
}