using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements ARCH4 cipher algorithm
    /// </summary>
    public class Arc4Cipher : StreamCipher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Arc4Cipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public Arc4Cipher(byte[] key)
            : base(key)
        {

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
        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            throw new NotImplementedException();
        }

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
        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public override byte[] Encrypt(byte[] input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public override byte[] Decrypt(byte[] input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validates the size of the key.
        /// </summary>
        /// <param name="keySize">Size of the key.</param>
        /// <returns>
        /// true if keySize is valid; otherwise false
        /// </returns>
        protected override bool ValidateKeySize(int keySize)
        {
            throw new NotImplementedException();
        }
    }
}
