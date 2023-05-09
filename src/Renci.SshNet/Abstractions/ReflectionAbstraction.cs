using System;
using System.Collections.Generic;
using System.Linq;

namespace Renci.SshNet.Abstractions
{
    internal static class ReflectionAbstraction
    {
        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit)
            where T:Attribute
        {
            var attributes = type.GetCustomAttributes(typeof(T), inherit);
            return attributes.Cast<T>();
        }
    }
}
