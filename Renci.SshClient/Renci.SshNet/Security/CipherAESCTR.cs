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
    public abstract class CipherAesCtr : Cipher
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
                return this._keySize / 8;
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
        /// Initializes a new instance of the <see cref="CipherAesCtr"/> class.
        /// </summary>
        /// <param name="keyBitsSize">Size of the key bits.</param>
        public CipherAesCtr(int keyBitsSize)
        {
            this._keySize = keyBitsSize;
        }

        /// <summary>
        /// Creates the encryptor.
        /// </summary>
        /// <returns></returns>
        protected override ModeBase CreateEncryptor()
        {
            return new CtrMode(new AesCipher(this.Key.Take(this._keySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray()));
        }

        /// <summary>
        /// Creates the decryptor.
        /// </summary>
        /// <returns></returns>
        protected override ModeBase CreateDecryptor()
        {
            return new CtrMode(new AesCipher(this.Key.Take(this._keySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray()));
        }
    }

    /// <summary>
    /// Represents AES 128 bit encryption.
    /// </summary>
    public class CipherAes128Ctr : CipherAesCtr
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "aes128-ctr"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherAes192Cbc"/> class.
        /// </summary>
        public CipherAes128Ctr()
            : base(128)
        {

        }
    }

    /// <summary>
    /// Represents AES 192 bit encryption.
    /// </summary>
    public class CipherAes192Ctr : CipherAesCtr
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "aes192-ctr"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherAes192Cbc"/> class.
        /// </summary>
        public CipherAes192Ctr()
            : base(192)
        {

        }
    }

    /// <summary>
    /// Represents AES 256 bit encryption.
    /// </summary>
    public class CipherAes256Ctr : CipherAesCtr
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "aes256-ctr"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherAes192Cbc"/> class.
        /// </summary>
        public CipherAes256Ctr()
            : base(256)
        {

        }
    }
}
