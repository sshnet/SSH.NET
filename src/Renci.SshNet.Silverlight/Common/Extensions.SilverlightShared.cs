using System;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Collection of different extension method specific for Silverlight
    /// </summary>
    internal static partial class Extensions
    {
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
    }
}
