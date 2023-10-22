using System;
using System.Globalization;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes
{
    /// <summary>
    /// Implements OFB cipher mode.
    /// </summary>
    public class OfbCipherMode : CipherMode
    {
        private readonly byte[] _ivOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="OfbCipherMode"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        public OfbCipherMode(byte[] iv)
            : base(iv)
        {
            _ivOutput = new byte[iv.Length];
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

            _ = Cipher.EncryptBlock(IV, 0, IV.Length, _ivOutput, 0);

            for (var i = 0; i < BlockSize; i++)
            {
                outputBuffer[outputOffset + i] = (byte)(_ivOutput[i] ^ inputBuffer[inputOffset + i]);
            }

            Buffer.BlockCopy(IV, BlockSize, IV, 0, IV.Length - BlockSize);
            Buffer.BlockCopy(outputBuffer, outputOffset, IV, IV.Length - BlockSize, BlockSize);

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
            return EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }
    }
}
