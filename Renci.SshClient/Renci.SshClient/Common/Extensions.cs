using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;

namespace Renci.SshClient
{
    public static class Extensions
    {
        /// <summary>
        /// Checks whether a collection is the same as another collection
        /// </summary>
        /// <param name="value">The current instance object</param>
        /// <param name="compareList">The collection to compare with</param>
        /// <param name="comparer">The comparer object to use to compare each item in the collection.  If null uses EqualityComparer(T).Default</param>
        /// <returns>True if the two collections contain all the same items in the same order</returns>
        public static bool IsEqualTo<TSource>(this IEnumerable<TSource> value, IEnumerable<TSource> compareList, IEqualityComparer<TSource> comparer)
        {
            if (value == compareList)
            {
                return true;
            }
            else if (value == null || compareList == null)
            {
                return false;
            }
            else
            {
                if (comparer == null)
                {
                    comparer = EqualityComparer<TSource>.Default;
                }

                IEnumerator<TSource> enumerator1 = value.GetEnumerator();
                IEnumerator<TSource> enumerator2 = compareList.GetEnumerator();

                bool enum1HasValue = enumerator1.MoveNext();
                bool enum2HasValue = enumerator2.MoveNext();

                try
                {
                    while (enum1HasValue && enum2HasValue)
                    {
                        if (!comparer.Equals(enumerator1.Current, enumerator2.Current))
                        {
                            return false;
                        }

                        enum1HasValue = enumerator1.MoveNext();
                        enum2HasValue = enumerator2.MoveNext();
                    }

                    return !(enum1HasValue || enum2HasValue);
                }
                finally
                {
                    if (enumerator1 != null) enumerator1.Dispose();
                    if (enumerator2 != null) enumerator2.Dispose();
                }
            }
        }

        public static bool IsEqualTo<TSource>(this IEnumerable<TSource> value, IEnumerable<TSource> compareList)
        {
            return IsEqualTo(value, compareList, null);
        }

        public static void DebugPrint(this IEnumerable<byte> bytes)
        {
            foreach (var b in bytes)
            {
                Debug.Write(string.Format("0x{0:x2}, ", b));
            }
            Debug.WriteLine(string.Empty);
        }

        public static string GetSshString(this IEnumerable<byte> data)
        {
            return new string((from b in data select (char)b).ToArray());
        }

        public static IEnumerable<byte> GetSshBytes(this string data)
        {
            foreach (var c in data)
            {
                yield return (byte)c;
            }
        }

        public static IEnumerable<byte> TrimLeadinZero(this IEnumerable<byte> data)
        {
            bool leadingZero = true;
            foreach (var item in data)
            {
                if (item == 0 & leadingZero)
                {
                    continue;
                }
                else
                {
                    leadingZero = false;
                }

                yield return item;
            }
        }

        /// <summary>
        /// Creates the instance of the type specified by the string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        internal static T CreateInstance<T>(this string name) where T : class
        {
            var type = Type.GetType(name);
            return Activator.CreateInstance(type) as T;
        }
    }
}
