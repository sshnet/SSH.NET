using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Collection of different extension method
    /// </summary>
    internal static partial class Extensions
    {
        internal static byte[] ToArray(this GlobalRequestName globalRequestName)
        {
            switch (globalRequestName)
            {
                case GlobalRequestName.TcpIpForward:
                    return SshData.Ascii.GetBytes("tcpip-forward");
                case GlobalRequestName.CancelTcpIpForward:
                    return SshData.Ascii.GetBytes("cancel-tcpip-forward");
                default:
                    throw new NotSupportedException(string.Format("Global request name '{0}' is not supported.", globalRequestName));
            }
        }

        internal static byte[] ToArray(this ServiceName serviceName)
        {
            switch (serviceName)
            {
                case ServiceName.UserAuthentication:
                    return SshData.Ascii.GetBytes("ssh-userauth");
                case ServiceName.Connection:
                    return SshData.Ascii.GetBytes("ssh-connection");
                default:
                    throw new NotSupportedException(string.Format("Service name '{0}' is not supported.", serviceName));
            }
        }

        internal static ServiceName ToServiceName(this byte[] data)
        {
            var sshServiceName = SshData.Ascii.GetString(data, 0, data.Length);
            switch (sshServiceName)
            {
                case "ssh-userauth":
                    return ServiceName.UserAuthentication;
                case "ssh-connection":
                    return ServiceName.Connection;
                default:
                    throw new NotSupportedException(string.Format("Service name '{0}' is not supported.", sshServiceName));
            }
        }

        internal static GlobalRequestName ToGlobalRequestName(this byte[] data)
        {
            var sshGlobalRequestName = SshData.Ascii.GetString(data, 0, data.Length);
            switch (sshGlobalRequestName)
            {
                case "tcpip-forward":
                    return GlobalRequestName.TcpIpForward;
                case "cancel-tcpip-forward":
                    return GlobalRequestName.CancelTcpIpForward;
                default:
                    throw new NotSupportedException(string.Format("Global request name '{0}' is not supported.", sshGlobalRequestName));
            }
        }

#if TUNING
        internal static BigInteger ToBigInteger(this byte[] data)
        {
            var reversed = new byte[data.Length];
            Buffer.BlockCopy(data, 0, reversed, 0, data.Length);
            return new BigInteger(reversed.Reverse());
        }

        /// <summary>
        /// Reverses the sequence of the elements in the entire one-dimensional <see cref="Array"/>.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> to reverse.</param>
        /// <returns>
        /// The <see cref="Array"/> with its elements reversed.
        /// </returns>
        internal static T[] Reverse<T>(this T[] array)
        {
            Array.Reverse(array);
            return array;
        }
#endif

        /// <summary>
        /// Checks whether a collection is the same as another collection
        /// </summary>
        /// <param name="value">The current instance object</param>
        /// <param name="compareList">The collection to compare with</param>
        /// <param name="comparer">The comparer object to use to compare each item in the collection.  If null uses EqualityComparer(T).Default</param>
        /// <returns>True if the two collections contain all the same items in the same order</returns>
        internal static bool IsEqualTo<TSource>(this IEnumerable<TSource> value, IEnumerable<TSource> compareList, IEqualityComparer<TSource> comparer)
        {
            if (value == compareList)
                return true;
            if (value == null || compareList == null)
                return false;

            if (comparer == null)
            {
                comparer = EqualityComparer<TSource>.Default;
            }

            var enumerator1 = value.GetEnumerator();
            var enumerator2 = compareList.GetEnumerator();

            var enum1HasValue = enumerator1.MoveNext();
            var enum2HasValue = enumerator2.MoveNext();

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
                enumerator1.Dispose();
                enumerator2.Dispose();
            }
        }

        /// <summary>
        /// Checks whether a collection is the same as another collection
        /// </summary>
        /// <param name="value">The current instance object</param>
        /// <param name="compareList">The collection to compare with</param>
        /// <returns>True if the two collections contain all the same items in the same order</returns>
        internal static bool IsEqualTo<TSource>(this IEnumerable<TSource> value, IEnumerable<TSource> compareList)
        {
            return IsEqualTo(value, compareList, null);
        }

#if SILVERLIGHT
#else

        /// <summary>
        /// Prints out 
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        internal static void DebugPrint(this IEnumerable<byte> bytes)
        {
            foreach (var b in bytes)
            {
                Debug.Write(string.Format(CultureInfo.CurrentCulture, "0x{0:x2}, ", b));
            }
            Debug.WriteLine(string.Empty);
        }
#endif

        /// <summary>
        /// Trims the leading zero from bytes array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Data without leading zeros.</returns>
        internal static IEnumerable<byte> TrimLeadingZero(this IEnumerable<byte> data)
        {
            var leadingZero = true;
            foreach (var item in data)
            {
                if (item == 0 & leadingZero)
                {
                    continue;
                }
                leadingZero = false;

                yield return item;
            }
        }

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="type">Type of the instance to create.</param>
        /// <returns>A reference to the newly created object.</returns>
        internal static T CreateInstance<T>(this Type type) where T : class
        {
            if (type == null)
                return null;
            return Activator.CreateInstance(type) as T;
        }

        /// <summary>
        /// Returns the specified 16-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        internal static byte[] GetBytes(this UInt16 value)
        {
            return new[] {(byte) (value >> 8), (byte) (value & 0xFF)};
        }

        /// <summary>
        /// Returns the specified 32-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        internal static byte[] GetBytes(this UInt32 value)
        {
#if TUNING
            var buffer = new byte[4];
            value.Write(buffer, 0);
            return buffer;
#else
            return new[] {(byte) (value >> 24), (byte) (value >> 16), (byte) (value >> 8), (byte) (value & 0xFF)};
#endif
        }

        /// <summary>
        /// Returns the specified 32-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="buffer">The array of bytes to write <paramref name="value"/> to.</param>
        /// <param name="offset">The zero-based offset in <paramref name="buffer"/> at which to begin writing.</param>
        internal static void Write(this uint value, byte[] buffer, int offset)
        {
            buffer[offset++] = (byte) (value >> 24);
            buffer[offset++] = (byte) (value >> 16);
            buffer[offset++] = (byte)(value >> 8);
            buffer[offset] = (byte) (value & 0xFF);
        }

        /// <summary>
        /// Returns the specified 64-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        internal static byte[] GetBytes(this UInt64 value)
        {
            return new[]
                {
                    (byte) (value >> 56), (byte) (value >> 48), (byte) (value >> 40), (byte) (value >> 32),
                    (byte) (value >> 24), (byte) (value >> 16), (byte) (value >> 8), (byte) (value & 0xFF)
                };
        }

        /// <summary>
        /// Returns the specified 64-bit signed integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        internal static byte[] GetBytes(this Int64 value)
        {
            return new[]
                {
                    (byte) (value >> 56), (byte) (value >> 48), (byte) (value >> 40), (byte) (value >> 32),
                    (byte) (value >> 24), (byte) (value >> 16), (byte) (value >> 8), (byte) (value & 0xFF)
                };
        }

        internal static void ValidatePort(this uint value, string argument)
        {
            if (value > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(argument,
                    string.Format(CultureInfo.InvariantCulture, "Specified value cannot be greater than {0}.",
                        IPEndPoint.MaxPort));
        }

        internal static void ValidatePort(this int value, string argument)
        {
            if (value < IPEndPoint.MinPort)
                throw new ArgumentOutOfRangeException(argument,
                    string.Format(CultureInfo.InvariantCulture, "Specified value cannot be less than {0}.",
                        IPEndPoint.MinPort));

            if (value > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(argument,
                    string.Format(CultureInfo.InvariantCulture, "Specified value cannot be greater than {0}.",
                        IPEndPoint.MaxPort));
        }

        /// <summary>
        /// Returns a specified number of contiguous bytes from a given offset.
        /// </summary>
        /// <param name="data">The array to return a number of bytes from.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin taking bytes.</param>
        /// <param name="length">The number of bytes to take from <paramref name="data"/>.</param>
        /// <returns>
        /// A <see cref="byte"/> array that contains the specified number of bytes at the specified offset
        /// of the input array.
        /// </returns>
        internal static byte[] Take(this byte[] data, int offset, int length)
        {
            var taken = new byte[length];
            Buffer.BlockCopy(data, offset, taken, 0, length);
            return taken;
        }
    }
}
