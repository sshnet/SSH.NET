#nullable enable
#if NETSTANDARD2_0 || NETFRAMEWORK
using System.Collections.Generic;
#endif

namespace System
{
    internal static class StringExtensions
    {
        extension(string text)
        {
#if NETSTANDARD2_0 || NETFRAMEWORK
            public static string Join(char separator, params string?[] value)
            {
                return string.Join(separator.ToString(), value);
            }

            public static string Join(char separator, IEnumerable<string?> value)
            {
                return string.Join(separator.ToString(), value);
            }

            public static string Join(char separator, string?[] value, int startIndex, int count)
            {
                return string.Join(separator.ToString(), value, startIndex, count);
            }

            public int IndexOf(char value, StringComparison comparisonType)
            {
                return text.IndexOf(value.ToString(), comparisonType);
            }
#endif
        }
    }
}
