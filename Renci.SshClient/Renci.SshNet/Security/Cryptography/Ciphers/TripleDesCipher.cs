using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements 3DES cipher algorithm.
    /// </summary>
    public class TripleDesCipher : DesCipher
    {
        private readonly int[] _encryptionKey1;
        private readonly int[] _encryptionKey2;
        private readonly int[] _encryptionKey3;

        private readonly int[] _decryptionKey1;
        private readonly int[] _decryptionKey2;
        private readonly int[] _decryptionKey3;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleDesCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="padding">The padding.</param>
        public TripleDesCipher(byte[] key, CipherMode mode, CipherPadding padding)
            : base(key, mode, padding)
        {
            var part1 = new byte[8];
            var part2 = new byte[8];

            Array.Copy(key, 0, part1, 0, 8);
            Array.Copy(key, 8, part2, 0, 8);

            this._encryptionKey1 = GenerateWorkingKey(true, part1);
            this._decryptionKey1 = GenerateWorkingKey(false, part1);

            this._encryptionKey2 = GenerateWorkingKey(false, part2);
            this._decryptionKey2 = GenerateWorkingKey(true, part2);

            if (key.Length == 24)
            {
                var part3 = new byte[8];
                Array.Copy(key, 16, part3, 0, 8);

                this._encryptionKey3 = GenerateWorkingKey(true, part3);
                this._decryptionKey3 = GenerateWorkingKey(false, part3);
            }
            else
            {
                this._encryptionKey3 = this._encryptionKey1;
                this._decryptionKey3 = this._decryptionKey1;
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
        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if ((inputOffset + this.BlockSize) > inputBuffer.Length)
                throw new IndexOutOfRangeException("input buffer too short");

            if ((outputOffset + this.BlockSize) > outputBuffer.Length)
                throw new IndexOutOfRangeException("output buffer too short");

            byte[] temp = new byte[this.BlockSize];

            DesCipher.DesFunc(this._encryptionKey1, inputBuffer, inputOffset, temp, 0);
            DesCipher.DesFunc(this._encryptionKey2, temp, 0, temp, 0);
            DesCipher.DesFunc(this._encryptionKey3, temp, 0, outputBuffer, outputOffset);

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

            byte[] temp = new byte[this.BlockSize];

            DesCipher.DesFunc(this._decryptionKey3, inputBuffer, inputOffset, temp, 0);
            DesCipher.DesFunc(this._decryptionKey2, temp, 0, temp, 0);
            DesCipher.DesFunc(this._decryptionKey1, temp, 0, outputBuffer, outputOffset);

            return this.BlockSize;
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
            if (keySize == 128 || keySize == 128 + 64)
                return true;
            else
                return false;
        }
    }
}
