using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;

namespace Renci.SshNet
{
    /// <summary>
    /// Collection of different extension method
    /// </summary>
    internal static partial class Extensions
    {
        /// <summary>
        /// Determines whether [is null or white space] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is null or white space] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (string.IsNullOrEmpty(value)) return true;

            return value.All(char.IsWhiteSpace);
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

        /// <summary>
        /// Prints out 
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        internal static void DebugPrint(this IEnumerable<byte> bytes)
        {
            var sb = new StringBuilder();

            foreach (var b in bytes)
            {
                sb.AppendFormat(CultureInfo.CurrentCulture, "0x{0:x2}, ", b);
            }
            Debug.WriteLine(sb.ToString());
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
        internal static byte[] GetBytes(this ushort value)
        {
            return new[] {(byte) (value >> 8), (byte) (value & 0xFF)};
        }

        /// <summary>
        /// Returns the specified 32-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        internal static byte[] GetBytes(this uint value)
        {
            var buffer = new byte[4];
            value.Write(buffer, 0);
            return buffer;
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
        internal static byte[] GetBytes(this ulong value)
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
        internal static byte[] GetBytes(this long value)
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
        /// <param name="value">The array to return a number of bytes from.</param>
        /// <param name="offset">The zero-based offset in <paramref name="value"/> at which to begin taking bytes.</param>
        /// <param name="count">The number of bytes to take from <paramref name="value"/>.</param>
        /// <returns>
        /// A <see cref="byte"/> array that contains the specified number of bytes at the specified offset
        /// of the input array.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <remarks>
        /// When <paramref name="offset"/> is zero and <paramref name="count"/> equals the length of <paramref name="value"/>,
        /// then <paramref name="value"/> is returned.
        /// </remarks>
        public static byte[] Take(this byte[] value, int offset, int count)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (count == 0)
                return Array<byte>.Empty;

            if (offset == 0 && value.Length == count)
                return value;

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
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <remarks>
        /// When <paramref name="count"/> equals the length of <paramref name="value"/>, then <paramref name="value"/>
        /// is returned.
        /// </remarks>
        public static byte[] Take(this byte[] value, int count)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (count == 0)
                return Array<byte>.Empty;

            if (value.Length == count)
                return value;

            var taken = new byte[count];
            Buffer.BlockCopy(value, 0, taken, 0, count);
            return taken;
        }

        public static bool IsEqualTo(this byte[] left, byte[] right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            if (left == right)
                return true;

            if (left.Length != right.Length)
                return false;

            for (var i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                    return false;
            }

            return true;
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
            if (value == null)
                throw new ArgumentNullException("value");

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == 0)
                    continue;

                // if the first byte is non-zero, then we return the byte array as is
                if (i == 0)
                    return value;

                var remainingBytes = value.Length - i;

                var cleaned = new byte[remainingBytes];
                Buffer.BlockCopy(value, i, cleaned, 0, remainingBytes);
                return cleaned;
            }

            return value;
        }

        public static byte[] Concat(this byte[] first, byte[] second)
        {
            if (first == null || first.Length == 0)
                return second;

            if (second == null || second.Length == 0)
                return first;

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
            if (socket == null)
                return false;
            return socket.Connected;
        }
    }
}
