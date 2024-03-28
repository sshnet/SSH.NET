using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Represents algorithm for Authenticated Encryption with Associated data.
    /// </summary>
    public abstract class AeadCipher : SymmetricCipher
    {
        /// <summary>
        /// Gets the size of the block in bytes.
        /// </summary>
        /// <value>
        /// The size of the block in bytes.
        /// </value>
        private readonly byte _blockSize;

        /// <summary>
        /// Gets the initial vector (nonce) for AEAD Encrypt and Decrypt.
        /// </summary>
        protected byte[] IV { get; }

        /// <summary>
        /// Gets the tag size in bytes.
        /// </summary>
        public int TagSize { get; }

        /// <inheritdoc/>
        public override byte MinimumSize
        {
            get { return _blockSize; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AeadCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The initial vector (nonce).</param>
        /// <param name="nonceSize">The nonce size in bytes.</param>
        /// <param name="tagSize">The tag size in bytes.</param>
        protected AeadCipher(byte[] key, byte[] iv, int nonceSize, int tagSize)
            : base(key)
        {
            _blockSize = 16;
            IV = iv.Take(nonceSize);
            TagSize = tagSize;
        }
    }
}
