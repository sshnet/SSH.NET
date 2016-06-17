using System;
using System.Collections.Generic;
using System.Linq;
#if FEATURE_REFLECTION_TYPEINFO
using System.Reflection;
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
            return new List<T>(attributes.Cast<T>());
#endif
        }
    }
}
