using System;

namespace dBosque.EntityFramework.Audit.Attributes
{
    /// <summary>
    /// The decorated Property is NotAuditable
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class NotAuditableAttribute : Attribute
    {
    }
}