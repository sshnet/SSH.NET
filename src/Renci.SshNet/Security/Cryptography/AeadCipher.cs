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
        /// <param name="tagSize">The tag size in bytes.</param>
        protected AeadCipher(byte[] key, int tagSize)
            : base(key)
        {
            _blockSize = 16;
            TagSize = tagSize;
        }
    }
}
