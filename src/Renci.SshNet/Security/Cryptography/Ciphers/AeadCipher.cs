using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Represents algorithm for Authenticated Encryption with Associated data.
    /// </summary>
    public abstract class AeadCipher : BlockCipher
    {
        /// <summary>
        /// Gets the initial vector (nonce) for AEAD Encrypt and Decrypt.
        /// </summary>
        protected byte[] IV { get; }

        /// <summary>
        /// Gets the tag size in bytes.
        /// </summary>
        public int TagSize { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AeadCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The initial vector (nonce).</param>
        /// <param name="nonceSize">The nonce size in bytes.</param>
        /// <param name="tagSize">The tag size in bytes.</param>
        protected AeadCipher(byte[] key, byte[] iv, int nonceSize, int tagSize)
            : base(key, blockSize: 16, mode: null, padding: null)
        {
            IV = iv.Take(nonceSize);
            TagSize = tagSize;
        }
    }
}
