using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents base class for DES-CBC encryption.
    /// </summary>
    public abstract class CipherDesCbc : Cipher
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
                return 0;
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
        /// Initializes a new instance of the <see cref="CipherDesCbc"/> class.
        /// </summary>
        /// <param name="keySize">Size of the key.</param>
        public CipherDesCbc(int keySize)
        {
            this._keySize = keySize;
        }

        /// <summary>
        /// Creates the encryptor.
        /// </summary>
        /// <returns></returns>
        protected override ModeBase CreateEncryptor()
        {
            return new CbcMode(new DesCipher(this.Key.Take(this.KeySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray()));
        }

        /// <summary>
        /// Creates the decryptor.
        /// </summary>
        /// <returns></returns>
        protected override ModeBase CreateDecryptor()
        {
            return new CbcMode(new DesCipher(this.Key.Take(this.KeySize / 8).ToArray(), this.Vector.Take(this.BlockSize).ToArray()));
        }
    }

    /// <summary>
    /// Represents class for DES-64 CBC encryption.
    /// </summary>
    public class CipherDes64Cbc : CipherDesCbc
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "des-cbc"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherDesCbc"/> class.
        /// </summary>
        public CipherDes64Cbc()
            : base(64)
        {

        }
    }

}
