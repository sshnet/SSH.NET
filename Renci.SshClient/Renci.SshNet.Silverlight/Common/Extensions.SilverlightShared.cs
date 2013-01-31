using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Collection of different extension method specific for Silverlight
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Determines whether [is null or white space] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is null or white space] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsNullOrWhiteSpace(this string value)
        {
            if (string.IsNullOrEmpty(value)) return true;

            return value.All(char.IsWhiteSpace);
        }

        /// <summary>
        /// Disposes the specified socket.
        /// </summary>
        /// <param name="socket">The socket.</param>
        [DebuggerNonUserCode]
        internal static void Dispose(this Socket socket)
        {
            if (socket == null)
                throw new NullReferenceException();

            socket.Close();
        }

        /// <summary>
        /// Disposes the specified handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        [DebuggerNonUserCode]
        internal static void Dispose(this WaitHandle handle)
        {
            if (handle == null)
                throw new NullReferenceException();

            handle.Close();
        }

        /// <summary>
        /// Disposes the specified algorithm.
        /// </summary>
        /// <param name="algorithm">The algorithm.</param>
        [DebuggerNonUserCode]
        internal static void Dispose(this HashAlgorithm algorithm)
        {
            if (algorithm == null)
                throw new NullReferenceException();

            algorithm.Clear();
        }

        internal static bool CanRead(this Socket socket)
        {
            return socket.Connected;
        }

        internal static bool CanWrite(this Socket socket)
        {
            return socket.Connected;
        }

        internal static IPAddress GetIPAddress(this string host)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(host, out ipAddress))
            {
                throw new ProxyException("Silverlight supports only IP addresses.");
            }
            return ipAddress;
        }
    }
}
