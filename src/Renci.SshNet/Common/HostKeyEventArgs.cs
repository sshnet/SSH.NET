using System;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Security;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for the HostKeyReceived event.
    /// </summary>
    public class HostKeyEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether host key can be trusted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if host key can be trusted; otherwise, <c>false</c>.
        /// </value>
        public bool CanTrust { get; set; }

        /// <summary>
        /// Gets the host key.
        /// </summary>
        public byte[] HostKey { get; private set; }

        /// <summary>
        /// Gets the host key name.
        /// </summary>
        public string HostKeyName{ get; private set; }

        /// <summary>
        /// Gets the finger print.
        /// </summary>
        public byte[] FingerPrint { get; private set; }

        /// <summary>
        /// Gets the length of the key in bits.
        /// </summary>
        /// <value>
        /// The length of the key in bits.
        /// </value>
        public int KeyLength { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostKeyEventArgs"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        public HostKeyEventArgs(KeyHostAlgorithm host)
        {
            CanTrust = true;   //  Set default value

            HostKey = host.Data;

            HostKeyName = host.Name;

            KeyLength = host.Key.KeyLength;

            using (var md5 = CryptoAbstraction.CreateMD5())
            {
                FingerPrint = md5.ComputeHash(host.Data);
            }
        }
    }
}
