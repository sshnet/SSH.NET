using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Represents the class for the CBC Block Cipher.
    /// </summary>
    public class CbcMode : ModeBase
    {
        private byte[] _iv;

        private byte[] _nextIV;

        private int _blockSize;

        /// <summary>
        /// Gets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        public override int BlockSize
        {
            get { return this._blockSize; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CbcMode"/> class.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        public CbcMode(CipherBase cipher)
            : base(cipher)
        {
            this._blockSize = cipher.BlockSize;
            this._iv = cipher.IV.Take(this._blockSize).ToArray();
            this._nextIV = new byte[this._iv.Length];
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

            for (int i = 0; i < this._blockSize; i++)
            {
                this._iv[i] ^= inputBuffer[inputOffset + i];
            }

            this.Cipher.EncryptBlock(this._iv, 0, inputCount, outputBuffer, outputOffset);

            Array.Copy(outputBuffer, outputOffset, this._iv, 0, this._iv.Length);

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

            Array.Copy(inputBuffer, inputOffset, this._nextIV, 0, this._nextIV.Length);

            this.Cipher.DecryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);

            for (int i = 0; i < this._blockSize; i++)
            {
                outputBuffer[outputOffset + i] ^= this._iv[i];
            }

            Array.Copy(this._nextIV, 0, this._iv, 0, this._nextIV.Length);

            return this._blockSize;
        }

    }
}
