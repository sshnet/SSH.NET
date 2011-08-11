using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for block cipher implementations.
    /// </summary>
    public abstract class BlockCipher : SymmetricCipher
    {
        private CipherMode _mode;

        private CipherPadding _padding;

        /// <summary>
        /// Gets the size of the block in bytes.
        /// </summary>
        /// <value>
        /// The size of the block in bytes.
        /// </value>
        public abstract int BlockSize { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="mode">Cipher mode.</param>
        /// <param name="padding">Cipher padding.</param>
        protected BlockCipher(byte[] key, CipherMode mode, CipherPadding padding)
            : base(key)
        {
            this._mode = mode;
            this._padding = padding;

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

            if (data.Length % this.BlockSize > 0)
            {
                if (this._padding == null)
                {
                    throw new ArgumentException("data");
                }
                else
                {
                    data = this._padding.Pad(this.BlockSize, data);
                }
            }

            var writtenBytes = 0;

            for (int i = 0; i < data.Length / this.BlockSize; i++)
            {
                if (this._mode == null)
                {
                    writtenBytes += this.EncryptBlock(data, i * this.BlockSize, this.BlockSize, output, i * this.BlockSize);
                }
                else
                {
                    writtenBytes += this._mode.EncryptBlock(data, i * this.BlockSize, this.BlockSize, output, i * this.BlockSize);
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
            if (data.Length % this.BlockSize > 0)
            {
                {
                    if (this._padding == null)
                    {
                        throw new ArgumentException("data");
                    }
                    else
                    {
                        data = this._padding.Pad(this.BlockSize, data);
                    }
                }
            }

            var output = new byte[data.Length];

            var writtenBytes = 0;
            for (int i = 0; i < data.Length / this.BlockSize; i++)
            {
                if (this._mode == null)
                {
                    writtenBytes += this.DecryptBlock(data, i * this.BlockSize, this.BlockSize, output, i * this.BlockSize);
                }
                else
                {
                    writtenBytes += this._mode.DecryptBlock(data, i * this.BlockSize, this.BlockSize, output, i * this.BlockSize);

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
