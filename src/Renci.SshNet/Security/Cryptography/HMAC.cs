using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Represents the info for Message Authentication Code (MAC).
    /// </summary>
    public sealed class HMAC : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HMAC"/> class.
        /// </summary>
        /// <param name="hashAlgorithm">The hash algorithm.</param>
        /// <param name="etm"><see langword="true"/> to enable encrypt-then-MAC, <see langword="false"/> to use encrypt-and-MAC.</param>
        public HMAC(
            HashAlgorithm hashAlgorithm,
            bool etm)
        {
            HashAlgorithm = hashAlgorithm;
            ETM = etm;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            HashAlgorithm?.Dispose();
        }

        /// <summary>
        /// Gets the hash algorithem.
        /// </summary>
        public HashAlgorithm HashAlgorithm { get; private set; }

        /// <summary>
        /// Gets a value indicating whether enable encryption-to-mac or encryption-then-mac.
        /// </summary>
        public bool ETM { get; private set; }
    }
}
