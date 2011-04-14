using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Security.Cryptography
{
    /// <summary>
    /// Represents the class for the OFB Block Cipher.
    /// </summary>
    public class OfbMode : ModeBase
    {
        private readonly byte[] _iv;

        private readonly byte[] _ivOutput;

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
        /// Initializes a new instance of the <see cref="OfbMode"/> class.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        public OfbMode(CipherBase cipher)
            : base(cipher)
        {
            this._blockSize = cipher.BlockSize;
            this._iv = cipher.IV.ToArray();
            this._ivOutput = new byte[this._iv.Length];
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
                throw new ArgumentException(string.Format("inputCount must be {0}.", this._blockSize));

            this.Cipher.EncryptBlock(this._iv, 0, this._iv.Length, this._ivOutput, 0);

            for (int i = 0; i < this._blockSize; i++)
            {
                outputBuffer[outputOffset + i] = (byte)(this._ivOutput[i] ^ inputBuffer[inputOffset + i]);
            }

            Array.Copy(this._iv, this._blockSize, this._iv, 0, this._iv.Length - this._blockSize);
            Array.Copy(outputBuffer, outputOffset, this._iv, this._iv.Length - this._blockSize, this._blockSize);

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
                throw new ArgumentException(string.Format("inputCount must be {0}.", this._blockSize));

            this.Cipher.EncryptBlock(this._iv, 0, this._iv.Length, this._ivOutput, 0);

            for (int i = 0; i < this._blockSize; i++)
            {
                outputBuffer[outputOffset + i] = (byte)(this._ivOutput[i] ^ inputBuffer[inputOffset + i]);
            }

            Array.Copy(this._iv, this._blockSize, this._iv, 0, this._iv.Length - this._blockSize);
            Array.Copy(outputBuffer, outputOffset, this._iv, this._iv.Length - this._blockSize, this._blockSize);

            return this._blockSize;
        }
    }
}
