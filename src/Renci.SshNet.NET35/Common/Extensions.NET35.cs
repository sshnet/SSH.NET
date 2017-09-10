using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Collection of different extension method specific for .NET 3.5
    /// </summary>
    internal static partial class Extensions
    {
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

        /// <summary>
        ///     Clears the contents of the string builder.
        /// </summary>
        /// <param name="value">
        ///     The <see cref="StringBuilder"/> to clear.
        /// </param>
        public static void Clear(this StringBuilder value)
        {
            value.Length = 0;
            value.Capacity = 16;
        }
    }
}
