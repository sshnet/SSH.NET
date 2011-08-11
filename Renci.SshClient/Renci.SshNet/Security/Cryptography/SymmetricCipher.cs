using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for symmetric cipher implementations.
    /// </summary>
    public abstract class SymmetricCipher : Cipher
    {
        /// <summary>
        /// Gets the size of the key in bits.
        /// </summary>
        /// <value>
        /// The size of the key in bits.
        /// </value>
        public int KeySize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        protected SymmetricCipher(byte[] key)
        {
            var keySize = key.Length * 8;

            if (this.ValidateKeySize(keySize))
            {
                this.KeySize = keySize;
            }
            else
            {
                throw new ArgumentException(string.Format("KeySize '{0}' is not valid for this algorithm.", keySize));
            }
        }

        /// <summary>
        /// Encrypts the specified region of the input byte array and copies the encrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to encrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write encrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes encrypted.
        /// </returns>
        public abstract int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        /// <summary>
        /// Decrypts the specified region of the input byte array and copies the decrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to decrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write decrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes decrypted.
        /// </returns>
        public abstract int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        /// <summary>
        /// Validates the size of the key.
        /// </summary>
        /// <param name="keySize">Size of the key.</param>
        /// <returns>true if keySize is valid; otherwise false</returns>
        protected abstract bool ValidateKeySize(int keySize);
    }
}
