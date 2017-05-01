using System;

namespace dBosque.EntityFramework.Audit.Attributes
{
    /// <summary>
    /// The decorated class or property is Auditable
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class AuditableAttribute: Attribute
    {
    }
}