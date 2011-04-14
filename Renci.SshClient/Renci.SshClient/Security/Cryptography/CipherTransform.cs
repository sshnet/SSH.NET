using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Renci.SshClient.Security.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    internal class CipherTransform : ICryptoTransform
    {
        private readonly int _blockSize;

        private readonly CipherBase _blockCipher;

        /// <summary>
        /// Gets the transform mode.
        /// </summary>
        public TransformMode TransformMode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherTransform"/> class.
        /// </summary>
        /// <param name="transformMode">The transform mode.</param>
        /// <param name="blockCipher">The block cipher.</param>
        public CipherTransform(TransformMode transformMode, CipherBase blockCipher)
        {
            this._blockCipher = blockCipher;

            this._blockSize = blockCipher.BlockSize;

            this.TransformMode = transformMode;
        }

        #region ICryptoTransform Members

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        /// <returns>true if the current transform can be reused; otherwise, false.</returns>
        public bool CanReuseTransform
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        /// <returns>true if multiple blocks can be transformed; otherwise, false.</returns>
        public bool CanTransformMultipleBlocks
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the input block size.
        /// </summary>
        /// <returns>The size of the input data blocks in bytes.</returns>
        public int InputBlockSize
        {
            get { return this._blockSize; }
        }

        /// <summary>
        /// Gets the output block size.
        /// </summary>
        /// <returns>The size of the output data blocks in bytes.</returns>
        public int OutputBlockSize
        {
            get { return this._blockSize; }
        }

        /// <summary>
        /// Transforms the specified region of the input byte array and copies the resulting transform to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write the transform.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes written.
        /// </returns>
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputCount % this._blockSize != 0)
                throw new ArgumentException("Invalid  value.");

            var length = 0;

            for (int i = 0; i < inputCount / this._blockSize; i++)
            {
                switch (this.TransformMode)
                {
                    case TransformMode.Encrypt:
                        length += this._blockCipher.EncryptBlock(inputBuffer, inputOffset + this._blockSize * i, this._blockSize, outputBuffer, outputOffset + this._blockSize * i);
                        break;
                    case TransformMode.Decrypt:
                        length += this._blockCipher.DecryptBlock(inputBuffer, inputOffset + this._blockSize * i, this._blockSize, outputBuffer, outputOffset + this._blockSize * i);
                        break;
                    default:
                        throw new ArgumentException(string.Format("TransformMode '{0}' is not supported.", this.TransformMode));
                }
            }

            return length;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            throw new NotSupportedException("TransformFinalBlock is currently not supported.");
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
