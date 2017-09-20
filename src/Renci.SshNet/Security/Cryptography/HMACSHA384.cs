#if FEATURE_HMAC_SHA384

using System.Security.Cryptography;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Computes a Hash-based Message Authentication Code (HMAC) by using the <see cref="SHA384"/> hash function.
    /// </summary>
    public class HMACSHA384 : System.Security.Cryptography.HMACSHA384
    {
        private readonly int _hashSize;

        /// <summary>
        /// Initializes a <see cref="HMACSHA384"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public HMACSHA384(byte[] key)
            : base(key)
        {
            _hashSize = base.HashSize;
        }

        /// <summary>
        /// Initializes a <see cref="HMACSHA384"/> with the specified key and size of the computed hash code.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="hashSize">The size, in bits, of the computed hash code.</param>
        public HMACSHA384(byte[] key, int hashSize)
            : base(key)
        {
            _hashSize = hashSize;
        }

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <value>
        /// The size, in bits, of the computed hash code.
        /// </value>
        public override int HashSize
        {
            get { return _hashSize; }
        }

        /// <summary>
        /// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <returns>
        /// The computed hash code.
        /// </returns>
        protected override byte[] HashFinal()
        {
            var hash = base.HashFinal();
            return hash.Take(HashSize / 8);
        }
    }
}

#endif // FEATURE_HMAC_SHA384
