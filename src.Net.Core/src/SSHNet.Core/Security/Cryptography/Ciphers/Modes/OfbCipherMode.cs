using System;
using System.Globalization;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes
{
    /// <summary>
    /// Implements OFB cipher mode
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
            this._ivOutput = new byte[iv.Length];
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
            if (inputBuffer.Length - inputOffset < this._blockSize)
                throw new ArgumentException("Invalid input buffer");

            if (outputBuffer.Length - outputOffset < this._blockSize)
                throw new ArgumentException("Invalid output buffer");

            if (inputCount != this._blockSize)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "inputCount must be {0}.", this._blockSize));

            this.Cipher.EncryptBlock(this.IV, 0, this.IV.Length, this._ivOutput, 0);

            for (int i = 0; i < this._blockSize; i++)
            {
                outputBuffer[outputOffset + i] = (byte)(this._ivOutput[i] ^ inputBuffer[inputOffset + i]);
            }

            Buffer.BlockCopy(this.IV, this._blockSize, this.IV, 0, this.IV.Length - this._blockSize);
            Buffer.BlockCopy(outputBuffer, outputOffset, this.IV, this.IV.Length - this._blockSize, this._blockSize);

            return this._blockSize;
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
            if (inputBuffer.Length - inputOffset < this._blockSize)
                throw new ArgumentException("Invalid input buffer");

            if (outputBuffer.Length - outputOffset < this._blockSize)
                throw new ArgumentException("Invalid output buffer");

            if (inputCount != this._blockSize)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "inputCount must be {0}.", this._blockSize));

            this.Cipher.EncryptBlock(this.IV, 0, this.IV.Length, this._ivOutput, 0);

            for (int i = 0; i < this._blockSize; i++)
            {
                outputBuffer[outputOffset + i] = (byte)(this._ivOutput[i] ^ inputBuffer[inputOffset + i]);
            }

            Buffer.BlockCopy(this.IV, this._blockSize, this.IV, 0, this.IV.Length - this._blockSize);
            Buffer.BlockCopy(outputBuffer, outputOffset, this.IV, this.IV.Length - this._blockSize, this._blockSize);

            return this._blockSize;
        }


    }
}
