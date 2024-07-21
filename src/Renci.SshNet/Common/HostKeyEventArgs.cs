using System;
using System.Security.Cryptography;

using Renci.SshNet.Security;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for the HostKeyReceived event.
    /// </summary>
    public class HostKeyEventArgs : EventArgs
    {
        private readonly Lazy<byte[]> _lazyFingerPrint;
        private readonly Lazy<string> _lazyFingerPrintSHA256;
        private readonly Lazy<string> _lazyFingerPrintMD5;

        /// <summary>
        /// Gets or sets a value indicating whether host key can be trusted.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if host key can be trusted; otherwise, <see langword="false"/>.
        /// </value>
        public bool CanTrust { get; set; }

        /// <summary>
        /// Gets the host key.
        /// </summary>
        public byte[] HostKey { get; private set; }

        /// <summary>
        /// Gets the host key name.
        /// </summary>
        public string HostKeyName { get; private set; }

        /// <summary>
        /// Gets the MD5 fingerprint.
        /// </summary>
        /// <value>
        /// MD5 fingerprint as byte array.
        /// </value>
        public byte[] FingerPrint
        {
            get
            {
                return _lazyFingerPrint.Value;
            }
        }

        /// <summary>
        /// Gets the SHA256 fingerprint of the host key in the same format as the ssh command,
        /// i.e. non-padded base64, but without the <c>SHA256:</c> prefix.
        /// </summary>
        /// <example><c>ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og</c>.</example>
        /// <value>
        /// Base64 encoded SHA256 fingerprint with padding (equals sign) removed.
        /// </value>
        public string FingerPrintSHA256
        {
            get
            {
                return _lazyFingerPrintSHA256.Value;
            }
        }

        /// <summary>
        /// Gets the MD5 fingerprint of the host key in the same format as the ssh command,
        /// i.e. hexadecimal bytes separated by colons, but without the <c>MD5:</c> prefix.
        /// </summary>
        /// <example><c>97:70:33:82:fd:29:3a:73:39:af:6a:07:ad:f8:80:49</c>.</example>
        public string FingerPrintMD5
        {
            get
            {
                return _lazyFingerPrintMD5.Value;
            }
        }

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
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <see langword="null"/>.</exception>
        public HostKeyEventArgs(KeyHostAlgorithm host)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            CanTrust = true;
            HostKey = host.Data;
            HostKeyName = host.Name;
            KeyLength = host.Key.KeyLength;

            _lazyFingerPrint = new Lazy<byte[]>(() =>
                {
#if NET6_0_OR_GREATER
                    return MD5.HashData(HostKey);
#else
                    using var md5 = MD5.Create();
                    return md5.ComputeHash(HostKey);
#endif
                });

            _lazyFingerPrintSHA256 = new Lazy<string>(() =>
                {
#if NET6_0_OR_GREATER
                    return Convert.ToBase64String(SHA256.HashData(HostKey))
#else
                    using var sha256 = SHA256.Create();

                    return Convert.ToBase64String(sha256.ComputeHash(HostKey))
#endif
#if NET || NETSTANDARD2_1_OR_GREATER
                                  .Replace("=", string.Empty, StringComparison.Ordinal);
#else
                                  .Replace("=", string.Empty);
#endif // NET || NETSTANDARD2_1_OR_GREATER
                });

            _lazyFingerPrintMD5 = new Lazy<string>(() =>
                {
#pragma warning disable CA1308 // Normalize strings to uppercase
                    return BitConverter.ToString(FingerPrint).Replace('-', ':').ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
                });
        }
    }
}
