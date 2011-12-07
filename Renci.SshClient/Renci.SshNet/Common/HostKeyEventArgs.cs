using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Security.Cryptography;

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
        /// Gets the finger print.
        /// </summary>
        public byte[] FingerPrint { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostKeyEventArgs"/> class.
        /// </summary>
        /// <param name="hostKey">The host key.</param>
        public HostKeyEventArgs(byte[] hostKey)
        {
            this.CanTrust = true;   //  Set default value

            this.HostKey = hostKey;

            using (var md5 = new MD5Hash())
            {
                this.FingerPrint = md5.ComputeHash(hostKey);
            }
        }
    }
}
