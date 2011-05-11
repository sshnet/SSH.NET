using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Renci.SshClient.Security.Cryptography
{
    /// <summary>
    /// Represents the class for the Blowfish algorithm.
    /// </summary>
    public class Blowfish : SymmetricAlgorithm
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Blowfish"/> class.
        /// </summary>
        public Blowfish()
            : this(16 * 8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blowfish"/> class.
        /// </summary>
        /// <param name="keySize">Size of the key.</param>
        public Blowfish(int keySize)
        {
            this.KeySizeValue = keySize;
            this.BlockSizeValue = 8 * 8;
            this.FeedbackSizeValue = BlockSizeValue;
            this.LegalBlockSizesValue = new KeySizes[] { new KeySizes(64, 64, 0 ) };
            this.LegalKeySizesValue = new KeySizes[] { new KeySizes(32, 448, 32) };
        }

        /// <summary>
        /// Creates a symmetric decryptor object with the specified <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key"/> property and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV"/>).
        /// </summary>
        /// <param name="rgbKey">The secret key to use for the symmetric algorithm.</param>
        /// <param name="rgbIV">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>
        /// A symmetric decryptor object.
        /// </returns>
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            base.ValidKeySize(rgbKey.Length * 8);

            return new CipherTransform(TransformMode.Decrypt, this.GetBlockCipher(new BlowfishCipher(rgbKey, rgbIV)));
        }

        /// <summary>
        /// Creates a symmetric encryptor object with the specified <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key"/> property and initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV"/>).
        /// </summary>
        /// <param name="rgbKey">The secret key to use for the symmetric algorithm.</param>
        /// <param name="rgbIV">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>
        /// A symmetric encryptor object.
        /// </returns>
        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            base.ValidKeySize(rgbKey.Length * 8);

            return new CipherTransform(TransformMode.Encrypt, this.GetBlockCipher(new BlowfishCipher(rgbKey, rgbIV)));
        }

        /// <summary>
        /// Generates a random initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV"/>) to use for the algorithm.
        /// </summary>
        public override void GenerateIV()
        {
            var random = new Random();
            this.KeyValue = new byte[this.KeySizeValue / 8];
            random.NextBytes(this.KeyValue);
        }

        /// <summary>
        /// Generates a random key (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key"/>) to use for the algorithm.
        /// </summary>
        public override void GenerateKey()
        {
            var random = new Random();
            this.IVValue = new byte[this.BlockSizeValue / 8];
            random.NextBytes(this.KeyValue);
        }

        private ModeBase GetBlockCipher(CipherBase cipher)
        {
            switch (this.Mode)
            {
                case CipherMode.CBC:
                    return new CbcMode(cipher);
                default:
                    throw new ArgumentException(string.Format("Mode '{0}' is not supported.", this.Mode));
            }
        }
    }
}
