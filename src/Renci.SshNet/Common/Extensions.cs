using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Messages;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Collection of different extension methods.
    /// </summary>
    internal static partial class Extensions
    {
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

        internal static BigInteger ToBigInteger(this byte[] data)
        {
            var reversed = new byte[data.Length];
            Buffer.BlockCopy(data, 0, reversed, 0, data.Length);
            return new BigInteger(reversed.Reverse());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> structure using the SSH BigNum2 Format.
        /// </summary>
        public static BigInteger ToBigInteger2(this byte[] data)
        {
            if ((data[0] & (1 << 7)) != 0)
            {
                var buf = new byte[data.Length + 1];
                Buffer.BlockCopy(data, 0, buf, 1, data.Length);
                data = buf;
            }

            return data.ToBigInteger();
        }

        public static byte[] ToByteArray(this BigInteger bigInt, bool isUnsigned = false, bool isBigEndian = false)
        {
            var data = bigInt.ToByteArray();

            if (isUnsigned && data[data.Length - 1] == 0)
            {
                data = data.Take(data.Length - 1);
            }

            if (isBigEndian)
            {
                _ = data.Reverse();
            }

            return data;
        }

        // See https://github.com/dotnet/runtime/blob/9b57a265c7efd3732b035bade005561a04767128/src/libraries/Common/src/System/Security/Cryptography/KeyBlobHelpers.cs#L51
        public static byte[] ExportKeyParameter(this BigInteger value, int length)
        {
            var target = value.ToByteArray(isUnsigned: true, isBigEndian: true);

            // The BCL crypto is expecting exactly-sized byte arrays (sized to "length").
            // If our byte array is smaller than required, then size it up.
            // Otherwise, just return as is: if it is too large, we'll let the BCL throw the error.
            if (target.Length < length)
            {
                var correctlySized = new byte[length];
                Buffer.BlockCopy(target, 0, correctlySized, length - target.Length, target.Length);
                return correctlySized;
            }

            return target;
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

        /// <summary>
        /// Prints out the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        internal static void DebugPrint(this IEnumerable<byte> bytes)
        {
            var sb = new StringBuilder();

            foreach (var b in bytes)
            {
                _ = sb.AppendFormat(CultureInfo.CurrentCulture, "0x{0:x2}, ", b);
            }

            Debug.WriteLine(sb.ToString());
        }

        internal static void ValidatePort(this uint value, string argument)
        {
            if (value > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException(argument,
                                                      string.Format(CultureInfo.InvariantCulture, "Specified value cannot be greater than {0}.", IPEndPoint.MaxPort));
            }
        }

        internal static void ValidatePort(this int value, string argument)
        {
            if (value < IPEndPoint.MinPort)
            {
                throw new ArgumentOutOfRangeException(argument, string.Format(CultureInfo.InvariantCulture, "Specified value cannot be less than {0}.", IPEndPoint.MinPort));
            }

            if (value > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException(argument, string.Format(CultureInfo.InvariantCulture, "Specified value cannot be greater than {0}.", IPEndPoint.MaxPort));
            }
        }

        /// <summary>
        /// Returns a specified number of contiguous bytes from a given offset.
        /// </summary>
        /// <param name="value">The array to return a number of bytes from.</param>
        /// <param name="offset">The zero-based offset in <paramref name="value"/> at which to begin taking bytes.</param>
        /// <param name="count">The number of bytes to take from <paramref name="value"/>.</param>
        /// <returns>
        /// A <see cref="byte"/> array that contains the specified number of bytes at the specified offset
        /// of the input array.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// When <paramref name="offset"/> is zero and <paramref name="count"/> equals the length of <paramref name="value"/>,
        /// then <paramref name="value"/> is returned.
        /// </remarks>
        public static byte[] Take(this byte[] value, int offset, int count)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (count == 0)
            {
                return Array.Empty<byte>();
            }

            if (offset == 0 && value.Length == count)
            {
                return value;
            }

            var taken = new byte[count];
            Buffer.BlockCopy(value, offset, taken, 0, count);
            return taken;
        }

        /// <summary>
        /// Returns a specified number of contiguous bytes from the start of the specified byte array.
        /// </summary>
        /// <param name="value">The array to return a number of bytes from.</param>
        /// <param name="count">The number of bytes to take from <paramref name="value"/>.</param>
        /// <returns>
        /// A <see cref="byte"/> array that contains the specified number of bytes at the start of the input array.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// When <paramref name="count"/> equals the length of <paramref name="value"/>, then <paramref name="value"/>
        /// is returned.
        /// </remarks>
        public static byte[] Take(this byte[] value, int count)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (count == 0)
            {
                return Array.Empty<byte>();
            }

            if (value.Length == count)
            {
                return value;
            }

            var taken = new byte[count];
            Buffer.BlockCopy(value, 0, taken, 0, count);
            return taken;
        }

        public static bool IsEqualTo(this byte[] left, byte[] right)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (left == right)
            {
                return true;
            }

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            return left.AsSpan().SequenceEqual(right);
#else
            if (left.Length != right.Length)
            {
                return false;
            }

            for (var i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
#endif
        }

        /// <summary>
        /// Trims the leading zero from a byte array.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <paramref name="value"/> without leading zeros.
        /// </returns>
        public static byte[] TrimLeadingZeros(this byte[] value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == 0)
                {
                    continue;
                }

                // if the first byte is non-zero, then we return the byte array as is
                if (i == 0)
                {
                    return value;
                }

                var remainingBytes = value.Length - i;

                var cleaned = new byte[remainingBytes];
                Buffer.BlockCopy(value, i, cleaned, 0, remainingBytes);
                return cleaned;
            }

            return value;
        }

#if NETFRAMEWORK || NETSTANDARD2_0
        public static int IndexOf(this byte[] array, byte[] value, int startIndex, int count)
        {
            if (value.Length > count)
            {
                return -1;
            }

            if (value.Length == 0)
            {
                return 0;
            }

            for (var i = startIndex; i < startIndex + count - value.Length + 1; i++)
            {
                if (MatchesAtIndex(i))
                {
                    return i - startIndex;
                }
            }

            return -1;

            bool MatchesAtIndex(int i)
            {
                for (var j = 0; j < value.Length; j++)
                {
                    if (array[i + j] != value[j])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
#endif

        /// <summary>
        /// Pads with leading zeros if needed.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="length">The length to pad to.</param>
        public static byte[] Pad(this byte[] data, int length)
        {
            if (length <= data.Length)
            {
                return data;
            }

            var newData = new byte[length];
            Buffer.BlockCopy(data, 0, newData, newData.Length - data.Length, data.Length);
            return newData;
        }

        public static byte[] Concat(this byte[] first, byte[] second)
        {
            if (first is null || first.Length == 0)
            {
                return second;
            }

            if (second is null || second.Length == 0)
            {
                return first;
            }

            var concat = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, concat, 0, first.Length);
            Buffer.BlockCopy(second, 0, concat, first.Length, second.Length);
            return concat;
        }

        internal static bool CanRead(this Socket socket)
        {
            return SocketAbstraction.CanRead(socket);
        }

        internal static bool CanWrite(this Socket socket)
        {
            return SocketAbstraction.CanWrite(socket);
        }

        internal static bool IsConnected(this Socket socket)
        {
            if (socket is null)
            {
                return false;
            }

            return socket.Connected;
        }
    }
}
