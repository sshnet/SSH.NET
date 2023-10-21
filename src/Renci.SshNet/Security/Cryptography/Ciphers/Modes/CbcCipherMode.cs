using System;
using System.Globalization;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes
{
    /// <summary>
    /// Implements CBC cipher mode.
    /// </summary>
    public class CbcCipherMode : CipherMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CbcCipherMode"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        public CbcCipherMode(byte[] iv)
            : base(iv)
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
            if (inputBuffer.Length - inputOffset < BlockSize)
            {
                throw new ArgumentException("Invalid input buffer");
            }

            if (outputBuffer.Length - outputOffset < BlockSize)
            {
                throw new ArgumentException("Invalid output buffer");
            }

            if (inputCount != BlockSize)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "inputCount must be {0}.", BlockSize));
            }

            for (var i = 0; i < BlockSize; i++)
            {
                IV[i] ^= inputBuffer[inputOffset + i];
            }

            _ = Cipher.EncryptBlock(IV, 0, inputCount, outputBuffer, outputOffset);

            Buffer.BlockCopy(outputBuffer, outputOffset, IV, 0, IV.Length);

            return BlockSize;
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
            if (inputBuffer.Length - inputOffset < BlockSize)
            {
                throw new ArgumentException("Invalid input buffer");
            }

            if (outputBuffer.Length - outputOffset < BlockSize)
            {
                throw new ArgumentException("Invalid output buffer");
            }

            if (inputCount != BlockSize)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "inputCount must be {0}.", BlockSize));
            }

            _ = Cipher.DecryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);

            for (var i = 0; i < BlockSize; i++)
            {
                outputBuffer[outputOffset + i] ^= IV[i];
            }

            Buffer.BlockCopy(inputBuffer, inputOffset, IV, 0, IV.Length);

            return BlockSize;
        }
    }
}
