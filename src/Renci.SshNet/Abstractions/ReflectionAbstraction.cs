using System;
using System.Collections.Generic;
#if FEATURE_REFLECTION_TYPEINFO
using System.Reflection;
#else
using System.Linq;
#endif // FEATURE_REFLECTION_TYPEINFO

namespace Renci.SshNet.Abstractions
{
    internal static class ReflectionAbstraction
    {
        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit)
            where T:Attribute
        {
#if FEATURE_REFLECTION_TYPEINFO
            return type.GetTypeInfo().GetCustomAttributes<T>(inherit);
#else
            var attributes = type.GetCustomAttributes(typeof(T), inherit);
            return attributes.Cast<T>();
#endif
        }
    }
}
