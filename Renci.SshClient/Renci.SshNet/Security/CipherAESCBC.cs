using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents base class for AES based encryption.
    /// </summary>
    public abstract class CipherAesCbc : Cipher
    {
        private readonly int _keySize;
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
                return this._keySize;
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
                return 16;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherAesCbc"/> class.
        /// </summary>
        /// <param name="keyBitsSize">Size of the key bits.</param>
        public CipherAesCbc(int keyBitsSize)
        {
            this._keySize = keyBitsSize;
        }

        /// <summary>
        /// Creates the encryptor.
        /// </summary>
        /// <returns></returns>
        protected override ModeBase CreateEncryptor()
        {
            return new CbcMode(new AesCipher(this.Key.Take(this._keySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray()));
        }

        /// <summary>
        /// Creates the decryptor.
        /// </summary>
        /// <returns></returns>
        protected override ModeBase CreateDecryptor()
        {
            return new CbcMode(new AesCipher(this.Key.Take(this._keySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray()));
        }
    }

    /// <summary>
    /// Represents AES 128 bit encryption.
    /// </summary>
    public class CipherAes128Cbc : CipherAesCbc
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "aes128-cbc"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherAes128Cbc"/> class.
        /// </summary>
        public CipherAes128Cbc()
            : base(128)
        {

        }
    }

    /// <summary>
    /// Represents AES 192 bit encryption.
    /// </summary>
    public class CipherAes192Cbc : CipherAesCbc
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "aes192-cbc"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherAes192Cbc"/> class.
        /// </summary>
        public CipherAes192Cbc()
            : base(192)
        {

        }
    }

    /// <summary>
    /// Represents AES 256 bit encryption.
    /// </summary>
    public class CipherAes256Cbc : CipherAesCbc
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "aes256-cbc"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherAes256Cbc"/> class.
        /// </summary>
        public CipherAes256Cbc()
            : base(256)
        {

        }
    }
}
