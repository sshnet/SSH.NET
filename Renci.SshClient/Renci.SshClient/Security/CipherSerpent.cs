using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents base class for Serpent based encryption.
    /// </summary>
    public abstract class CipherSerpentCBC : Cipher
    {
        private SymmetricAlgorithm _algorithm;

        private ICryptoTransform _encryptor;

        private ICryptoTransform _decryptor;

        /// <summary>
        /// Gets or sets the key size, in bits, of the secret key used by the cipher.
        /// </summary>
        /// <value>
        /// The key size, in bits.
        /// </value>
        public override int KeySize
        {
            get
            {
                return this._algorithm.KeySize;
            }
        }

        /// <summary>
        /// Gets or sets the block size, in bits, of the cipher operation.
        /// </summary>
        /// <value>
        /// The block size, in bits.
        /// </value>
        public override int BlockSize
        {
            get
            {
                return this._algorithm.BlockSize / 8;
            }
        }

        /// <summary>
        /// Gets the size of the key bits.
        /// </summary>
        /// <value>
        /// The size of the key bits.
        /// </value>
        protected int KeyBitsSize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherSerpentCBC"/> class.
        /// </summary>
        /// <param name="keyBitsSize">Size of the key bits.</param>
        public CipherSerpentCBC(int keyBitsSize)
        {
            this.KeyBitsSize = keyBitsSize;
            this._algorithm = new Cryptography.Serpent(keyBitsSize);
            this._algorithm.KeySize = keyBitsSize;
            this._algorithm.Mode = System.Security.Cryptography.CipherMode.CBC;
            this._algorithm.Padding = System.Security.Cryptography.PaddingMode.None;
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// Encrypted data
        /// </returns>
        public override byte[] Encrypt(byte[] data)
        {
            if (this._encryptor == null)
            {
                this._encryptor = this._algorithm.CreateEncryptor(this.Key.Take(this.KeySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray());
            }

            var output = new byte[data.Length];
            var writtenBytes = this._encryptor.TransformBlock(data, 0, data.Length, output, 0);

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
        /// <returns>
        /// Decrypted data
        /// </returns>
        public override byte[] Decrypt(byte[] data)
        {
            if (this._decryptor == null)
            {
                this._decryptor = this._algorithm.CreateDecryptor(this.Key.Take(this.KeySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray());
            }

            var output = new byte[data.Length];
            var writtenBytes = this._decryptor.TransformBlock(data, 0, data.Length, output, 0);

            if (writtenBytes < data.Length)
            {
                throw new InvalidOperationException("Encryption error.");
            }
            return output;
        }

        private bool _isDisposed = false;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._algorithm != null)
                    {
                        this._algorithm.Dispose();
                        this._algorithm = null;
                    }
                }

                // Note disposing has been done.
                this._isDisposed = true;
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Represents Serpent 128 bit encryption.
    /// </summary>
    public class CipherSerpent128CBC : CipherSerpentCBC
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "serpent128-cbc"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherSerpent128CBC"/> class.
        /// </summary>
        public CipherSerpent128CBC()
            : base(128)
        {

        }
    }

    /// <summary>
    /// Represents Serpent 192 bit encryption.
    /// </summary>
    public class CipherSerpent192CBC : CipherSerpentCBC
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "serpent192-cbc"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherSerpent192CBC"/> class.
        /// </summary>
        public CipherSerpent192CBC()
            : base(192)
        {

        }
    }

    /// <summary>
    /// Represents Serpent 256 bit encryption.
    /// </summary>
    public class CipherSerpent256CBC : CipherSerpentCBC
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "serpent256-cbc"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherSerpent256CBC"/> class.
        /// </summary>
        public CipherSerpent256CBC()
            : base(256)
        {

        }
    }

}
