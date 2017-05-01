using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace dBosque.EntityFramework.Audit
{
    internal static class Extensions
    {
        #region ReflectionExt
        public static bool IsAttr<T>(this PropertyInfo entry) where T : Attribute
        {
            return entry.CustomAttributes.Any(q => q.AttributeType == typeof(T));
        }

        public static bool IsAttr<T>(this Type type) where T : Attribute
        {
            return type.CustomAttributes.Any(q => q.AttributeType == typeof(T));
        }

        public static bool IsAttr<T>(this DbEntityEntry entry) where T : Attribute
        {
            var entity = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(entry.Entity.GetType());
            return entity.CustomAttributes.Any(q => q.AttributeType == typeof(T));
        }

        #endregion
    }
}