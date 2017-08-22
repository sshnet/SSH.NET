using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private enum ShellQuoteState
        {
            Unquoted = 1,
            SingleQuoted = 2,
            Quoted = 3
        }

        /// <summary>
        /// Quotes a <see cref="string"/> in a way to be suitable to be used with a shell.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to quote.</param>
        /// <returns>
        /// A quoted <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <remarks>
        /// <para>
        /// If <paramref name="value"/> contains a single-quote, that character is embedded
        /// in quotation marks (eg. "'"). Sequences of single-quotes are grouped in a one
        /// pair of quotation marks.
        /// </para>
        /// <para>
        /// If the <see cref="string"/> contains an exclamation mark (!), the C-Shell interprets
        /// it as a meta-character for history substitution. This even works inside single-quotes
        /// or quotation marks, unless escaped with a backslash (\).
        /// </para>
        /// <para>
        /// References:
        /// <list type="bullet">
        ///   <item>
        ///     <description><a href="http://pubs.opengroup.org/onlinepubs/7908799/xcu/chap2.html">Shell Command Language</a></description>
        ///   </item>
        ///   <item>
        ///     <description><a href="https://earthsci.stanford.edu/computing/unix/shell/specialchars.php">Unix C-Shell special characters and their uses</a></description>
        ///   </item>
        ///   <item>
        ///     <description><a href="https://docstore.mik.ua/orelly/unix3/upt/ch27_13.htm">Differences Between Bourne and C Shell Quoting</a></description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        public static string ShellQuote(this string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            // result is at least value and leading/trailing single-quote
            var sb = new StringBuilder(value.Length + 2);
            var state = ShellQuoteState.Unquoted;

            foreach (var c in value)
            {
                switch (c)
                {
                    case '\'':
                        // embed a single-quote in quotes
                        switch (state)
                        {
                            case ShellQuoteState.Unquoted:
                                // Start quoted string
                                sb.Append('"');
                                break;
                            case ShellQuoteState.Quoted:
                                // Continue quoted string
                                break;
                            case ShellQuoteState.SingleQuoted:
                                // Close single quoted string
                                sb.Append('\'');
                                // Start quoted string
                                sb.Append('"');
                                break;
                        }
                        state = ShellQuoteState.Quoted;
                        break;
                    case '!':
                        // In C-Shell, an exclamatation point can only be protected from shell interpretation
                        // when escaped by a backslash
                        // Source:
                        // https://earthsci.stanford.edu/computing/unix/shell/specialchars.php

                        switch (state)
                        {
                            case ShellQuoteState.Unquoted:
                                sb.Append('\\');
                                break;
                            case ShellQuoteState.Quoted:
                                // Close quoted string
                                sb.Append('"');
                                sb.Append('\\');
                                break;
                            case ShellQuoteState.SingleQuoted:
                                // Close single quoted string
                                sb.Append('\'');
                                sb.Append('\\');
                                break;
                        }
                        state = ShellQuoteState.Unquoted;
                        break;
                    default:
                        switch (state)
                        {
                            case ShellQuoteState.Unquoted:
                                // Start single-quoted string
                                sb.Append('\'');
                                break;
                            case ShellQuoteState.Quoted:
                                // Close quoted string
                                sb.Append('"');
                                // Start single quoted string
                                sb.Append('\'');
                                break;
                            case ShellQuoteState.SingleQuoted:
                                // Continue single quoted string
                                break;
                        }
                        state = ShellQuoteState.SingleQuoted;
                        break;
                }

                sb.Append(c);
            }

            switch (state)
            {
                case ShellQuoteState.Unquoted:
                    break;
                case ShellQuoteState.Quoted:
                    // Close quoted string
                    sb.Append('"');
                    break;
                case ShellQuoteState.SingleQuoted:
                    /* Close single quoted string */
                    sb.Append('\'');
                    break;
            }

            if (sb.Length == 0)
            {
                sb.Append("''");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether the specified value is null or white space.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> is null or white space; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (string.IsNullOrEmpty(value)) return true;

            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }

            return true;
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
