using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renci.SshClient.Security.Cryptography
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

        public CbcMode(CipherBase cipher)
            : base(cipher)
        {
            this._blockSize = cipher.BlockSize;
            this._iv = cipher.IV.Take(this._blockSize).ToArray();
            this._nextIV = new byte[this._iv.Length];
        }

        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer.Length - inputOffset < this._blockSize)
                throw new ArgumentException("Invalid input buffer");

            if (outputBuffer.Length - outputOffset < this._blockSize)
                throw new ArgumentException("Invalid output buffer");

            if (inputCount != this._blockSize)
                throw new ArgumentException(string.Format("inputCount must be {0}.", this._blockSize));

            for (int i = 0; i < this._blockSize; i++)
            {
                this._iv[i] ^= inputBuffer[inputOffset + i];
            }

            this.Cipher.EncryptBlock(this._iv, 0, inputCount, outputBuffer, outputOffset);

            Array.Copy(outputBuffer, outputOffset, this._iv, 0, this._iv.Length);

            return this._blockSize;
        }

        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer.Length - inputOffset < this._blockSize)
                throw new ArgumentException("Invalid input buffer");

            if (outputBuffer.Length - outputOffset < this._blockSize)
                throw new ArgumentException("Invalid output buffer");

            if (inputCount != this._blockSize)
                throw new ArgumentException(string.Format("inputCount must be {0}.", this._blockSize));

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
