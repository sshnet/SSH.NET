using System;
using System.Globalization;
using System.Linq;
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
        /// Gets the MD5 fingerprint of the host key.
        /// </summary>
        public byte[] FingerPrint { get; private set; }

        /// <summary>
        /// Gets the MD5 fingerprint of the host key in the same format as the ssh command,
        /// i.e. hexadecimal bytes seperated by colons, but without the <c>MD5:</c> prefix.
        /// </summary>
        /// <example><c>97:70:33:82:fd:29:3a:73:39:af:6a:07:ad:f8:80:49</c></example>
        public string MD5FingerPrint { get; private set; }

        /// <summary>
        /// Gets the SHA256 fingerprint of the host key in the same format as the ssh command,
        /// i.e. non-padded base64, but without the <c>SHA256:</c> prefix.
        /// </summary>
        /// <example><c>ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og</c></example>
        public string SHA256FingerPrint { get; private set; }

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
            CanTrust = true;
            HostKey = host.Data;
            HostKeyName = host.Name;
            KeyLength = host.Key.KeyLength;

            using (var md5 = CryptoAbstraction.CreateMD5())
            {
                FingerPrint = md5.ComputeHash(host.Data);
                MD5FingerPrint = string.Join(":", FingerPrint.Select(e => e.ToString("x2", NumberFormatInfo.InvariantInfo)));
            }
            using (var sha256 = CryptoAbstraction.CreateSHA256())
            {
                SHA256FingerPrint = Convert.ToBase64String(sha256.ComputeHash(host.Data)).TrimEnd('=');
            }
        }
    }
}
