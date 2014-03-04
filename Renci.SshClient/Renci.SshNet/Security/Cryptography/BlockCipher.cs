using System;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for block cipher implementations.
    /// </summary>
    public abstract class BlockCipher : SymmetricCipher
    {
        private readonly CipherMode _mode;

        private readonly CipherPadding _padding;

        /// <summary>
        /// Gets the size of the block in bytes.
        /// </summary>
        /// <value>
        /// The size of the block in bytes.
        /// </value>
        protected readonly byte _blockSize;

        /// <summary>
        /// Gets the minimum data size.
        /// </summary>
        /// <value>
        /// The minimum data size.
        /// </value>
        public override byte MinimumSize
        {
            get { return this.BlockSize; }
        }

        /// <summary>
        /// Gets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        public byte BlockSize
        {
            get
            {
                return this._blockSize;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="mode">Cipher mode.</param>
        /// <param name="padding">Cipher padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        protected BlockCipher(byte[] key, byte blockSize, CipherMode mode, CipherPadding padding)
            : base(key)
        {
            this._blockSize = blockSize;
            this._mode = mode;
            this._padding = padding;

            if (this._mode != null)
                this._mode.Init(this);
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Encrypted data</returns>
        public override byte[] Encrypt(byte[] data)
        {
            var output = new byte[data.Length];

            if (data.Length % this._blockSize > 0)
            {
                if (this._padding == null)
                {
                    throw new ArgumentException("data");
                }
                data = this._padding.Pad(this._blockSize, data);
            }

            var writtenBytes = 0;

            for (int i = 0; i < data.Length / this._blockSize; i++)
            {
                if (this._mode == null)
                {
                    writtenBytes += this.EncryptBlock(data, i * this._blockSize, this._blockSize, output, i * this._blockSize);
                }
                else
                {
                    writtenBytes += this._mode.EncryptBlock(data, i * this._blockSize, this._blockSize, output, i * this._blockSize);
                }
            }

            if (writtenBytes < data.Length)
            {
                throw new InvalidOperationException("Encryption error.");
            }

            return output;
        }

        /// <summary>
        /// Decrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Decrypted data</returns>
        public override byte[] Decrypt(byte[] data)
        {
            if (data.Length % this._blockSize > 0)
            {
                {
                    if (this._padding == null)
                    {
                        throw new ArgumentException("data");
                    }
                    data = this._padding.Pad(this._blockSize, data);
                }
            }

            var output = new byte[data.Length];

            var writtenBytes = 0;
            for (int i = 0; i < data.Length / this._blockSize; i++)
            {
                if (this._mode == null)
                {
                    writtenBytes += this.DecryptBlock(data, i * this._blockSize, this._blockSize, output, i * this._blockSize);
                }
                else
                {
                    writtenBytes += this._mode.DecryptBlock(data, i * this._blockSize, this._blockSize, output, i * this._blockSize);
                }
            }

            if (writtenBytes < data.Length)
            {
                throw new InvalidOperationException("Encryption error.");
            }

            return output;
        }
    }
}
