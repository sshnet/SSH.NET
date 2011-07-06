using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents base class for Serpent based encryption.
    /// </summary>
    public abstract class CipherSerpentCBC : Cipher
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
        /// Initializes a new instance of the <see cref="CipherSerpentCBC"/> class.
        /// </summary>
        /// <param name="keyBitsSize">Size of the key bits.</param>
        public CipherSerpentCBC(int keyBitsSize)
        {
            this._keySize = keyBitsSize;
        }

        /// <summary>
        /// Creates the encryptor.
        /// </summary>
        /// <returns></returns>
        protected override ModeBase CreateEncryptor()
        {
            return new CbcMode(new SerpentCipher(this.Key.Take(this.KeySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray()));
        }

        /// <summary>
        /// Creates the decryptor.
        /// </summary>
        /// <returns></returns>
        protected override ModeBase CreateDecryptor()
        {
            return new CbcMode(new SerpentCipher(this.Key.Take(this.KeySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray()));
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
