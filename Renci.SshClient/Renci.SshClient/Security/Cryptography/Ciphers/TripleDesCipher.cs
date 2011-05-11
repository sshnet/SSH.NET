using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Renci.SshClient.Security.Cryptography
{
    /// <summary>
    /// Represents the class for the 3DES algorithm.
    /// </summary>
    public class TripleDesCipher : DesCipher
    {
        private byte[] _pass1;
        private byte[] _pass2;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleDesCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        public TripleDesCipher(byte[] key, byte[] iv)
            : base(key, iv)
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
            if ((inputOffset + this.BlockSize) > inputBuffer.Length)
                throw new IndexOutOfRangeException("input buffer too short");

            if ((outputOffset + this.BlockSize) > outputBuffer.Length)
                throw new IndexOutOfRangeException("output buffer too short");

            if (this._pass1 == null)
                this._pass1 = new byte[this.BlockSize];
            if (this._pass2 == null)
                this._pass2 = new byte[this.BlockSize];

            DesCipher.DesFunc(this.EncryptionKey, inputBuffer, inputOffset, this._pass1, 0);
            DesCipher.DesFunc(this.EncryptionKey, this._pass1, 0, this._pass2, 0);
            DesCipher.DesFunc(this.EncryptionKey, this._pass2, 0, outputBuffer, outputOffset);

            return this.BlockSize;
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
            if ((inputOffset + this.BlockSize) > inputBuffer.Length)
                throw new IndexOutOfRangeException("input buffer too short");

            if ((outputOffset + this.BlockSize) > outputBuffer.Length)
                throw new IndexOutOfRangeException("output buffer too short");

            DesCipher.DesFunc(this.DecryptionKey, inputBuffer, inputOffset, this._pass1, 0);
            DesCipher.DesFunc(this.DecryptionKey, this._pass1, 0, this._pass2, 0);
            DesCipher.DesFunc(this.DecryptionKey, this._pass2, 0, outputBuffer, outputOffset);

            return this.BlockSize;
        }
    }
}
