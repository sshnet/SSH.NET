using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;

namespace Renci.SshNet.Security.Cryptography
{
    //  TODO:   TripleDes algorithm is not tested yet but fully implemented
    /// <summary>
    /// 
    /// </summary>
    public class TripleDes : SymmetricAlgorithm
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TripleDes"/> class.
        /// </summary>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">The implementation of the class derived from the symmetric algorithm is not valid.</exception>
        public TripleDes()
            : this(256)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleDes"/> class.
        /// </summary>
        /// <param name="keySize">Size of the key.</param>
        public TripleDes(int keySize)
        {
            this.KeySizeValue = keySize;
            this.BlockSizeValue = 128;
            this.FeedbackSizeValue = BlockSizeValue;
            this.LegalBlockSizesValue = new KeySizes[] { new KeySizes(128, 256, 64) };
            this.LegalKeySizesValue = new KeySizes[] { new KeySizes(128, 256, 64) };
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

            return new CipherTransform(TransformMode.Decrypt, this.GetBlockCipher(new TripleDesCipher(rgbKey, rgbIV)));
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

            return new CipherTransform(TransformMode.Encrypt, this.GetBlockCipher(new TripleDesCipher(rgbKey, rgbIV)));
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
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Mode '{0}' is not supported.", this.Mode));
            }
        }

    }
}
