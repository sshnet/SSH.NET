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
        /// Initializes a new instance of the <see cref="HMACSHA384"/> class with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public HMACSHA384(byte[] key)
            : base(key)
        {
#pragma warning disable MA0056 // Do not call overridable members in constructor
            _hashSize = base.HashSize;
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HMACSHA384"/> class with the specified key
        /// and size of the computed hash code.
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
