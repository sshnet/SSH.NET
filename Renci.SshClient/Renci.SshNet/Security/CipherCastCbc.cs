using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents base class for CAST-CBC based encryption.
    /// </summary>
    public abstract class CipherCastCbc : Cipher
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
                return 8;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherCastCbc"/> class.
        /// </summary>
        /// <param name="keySize">Size of the key.</param>
        public CipherCastCbc(int keySize)
        {
            this._keySize = keySize;
        }

        /// <summary>
        /// Creates the encryptor.
        /// </summary>
        /// <returns></returns>
        protected override ModeBase CreateEncryptor()
        {
            return new CbcMode(new CastCipher(this.Key.Take(this.KeySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray()));
        }

        /// <summary>
        /// Creates the decryptor.
        /// </summary>
        /// <returns></returns>
        protected override ModeBase CreateDecryptor()
        {
            return new CbcMode(new CastCipher(this.Key.Take(this.KeySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray()));
        }
    }

    /// <summary>
    /// Represents class for CAST 128 CBC based encryption.
    /// </summary>
    public class CipherCast128Cbc : CipherCastCbc
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "cast128-cbc"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherCast128Cbc"/> class.
        /// </summary>
        public CipherCast128Cbc()
            : base(128)
        {

        }        
    }
}
