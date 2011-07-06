using System.Collections.Generic;
using System.Linq;
using System;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents the abstract base class from which all implementations of cipher must inherit.
    /// </summary>
    public abstract class Cipher : Algorithm
    {
        private ModeBase _encryptor;

        private ModeBase _decryptor;

        /// <summary>
        /// Gets or sets the block size, in bits, of the cipher operation.
        /// </summary>
        /// <value>
        /// The block size, in bits.
        /// </value>
        public abstract int BlockSize { get; }

        /// <summary>
        /// Gets or sets the key size, in bits, of the secret key used by the cipher.
        /// </summary>
        /// <value>
        /// The key size, in bits.
        /// </value>
        public abstract int KeySize { get; }

        /// <summary>
        /// Gets the secret key for the cipher.
        /// </summary>
        protected byte[] Key { get; private set; }

        /// <summary>
        /// Gets the initialization vector (IV) for the cipher.
        /// </summary>
        protected byte[] Vector { get; private set; }

        /// <summary>
        /// Initializes the cipher.
        /// </summary>
        /// <param name="key">The secret key.</param>
        /// <param name="vector">The initialization vector.</param>
        public virtual void Init(IEnumerable<byte> key, IEnumerable<byte> vector)
        {
            this.Key = key.ToArray();
            this.Vector = vector.ToArray();
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Encrypted data</returns>
        public byte[] Encrypt(byte[] data)
        {
            if (this._encryptor == null)
            {
                this._encryptor = this.CreateEncryptor();
            }

            var output = new byte[data.Length];

            if (data.Length % this.BlockSize > 0)
                throw new ArgumentException("data");

            var writtenBytes = 0;
            for (int i = 0; i < data.Length / this.BlockSize; i++)
            {
                writtenBytes += this._encryptor.EncryptBlock(data, i * this.BlockSize, this.BlockSize, output, i * this.BlockSize);
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
        public byte[] Decrypt(byte[] data)
        {
            if (this._decryptor == null)
            {
                this._decryptor = this.CreateDecryptor();
            }

            var output = new byte[data.Length];

            if (data.Length % this.BlockSize > 0)
                throw new ArgumentException("data");

            var writtenBytes = 0;
            for (int i = 0; i < data.Length / this.BlockSize; i++)
            {
                writtenBytes += this._decryptor.DecryptBlock(data, i * this.BlockSize, this.BlockSize, output, i * this.BlockSize);
            }

            if (writtenBytes < data.Length)
            {
                throw new InvalidOperationException("Encryption error.");
            }
            return output;
        }

        /// <summary>
        /// Creates the encryptor.
        /// </summary>
        /// <returns></returns>
        protected abstract ModeBase CreateEncryptor();

        /// <summary>
        /// Creates the decryptor.
        /// </summary>
        /// <returns></returns>
        protected abstract ModeBase CreateDecryptor();
    }
}
