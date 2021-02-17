using System;
using System.Globalization;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes
{
    /// <summary>
    /// Implements CTR cipher mode
    /// </summary>
    public class AEADCipherMode : CipherMode
    {
        private readonly byte[] _ivOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="AEADCipherMode"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        public AEADCipherMode(byte[] iv)
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
            if (inputBuffer.Length - inputOffset < _blockSize)
                throw new ArgumentException("Invalid input buffer");

            if (outputBuffer.Length - outputOffset < _blockSize)
                throw new ArgumentException("Invalid output buffer");

            if (inputCount != _blockSize)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "inputCount must be {0}.", _blockSize));

            Cipher.EncryptBlock(IV, 0, IV.Length, _ivOutput, 0);

            for (int i = 0; i < _blockSize; i++)
            {
                outputBuffer[outputOffset + i] = (byte)(_ivOutput[i] ^ inputBuffer[inputOffset + i]);
            }

            int j = IV.Length;
            while (--j >= 0 && ++IV[j] == 0) ;

            return _blockSize;
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
            if (inputBuffer.Length - inputOffset < _blockSize)
                throw new ArgumentException("Invalid input buffer");

            if (outputBuffer.Length - outputOffset < _blockSize)
                throw new ArgumentException("Invalid output buffer");

            if (inputCount != _blockSize)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "inputCount must be {0}.", _blockSize));

            Cipher.EncryptBlock(IV, 0, IV.Length, _ivOutput, 0);

            for (int i = 0; i < _blockSize; i++)
            {
                outputBuffer[outputOffset + i] = (byte)(_ivOutput[i] ^ inputBuffer[inputOffset + i]);
            }

            int j = IV.Length;
            while (--j >= 0 && ++IV[j] == 0) ;

            return _blockSize;
        }

        /// <summary>
        /// Encrypts the specified region of the input byte array and copies the encrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputIV">The initial vector.</param>
        /// <param name="IVLen">The offset into the input byte array from which to begin using data.</param>
        /// <param name="add">The Additional authenticated data.</param>
        /// <param name="aLen">The Length of the additional data.</param>
        /// <param name="p">The Plaintext to be encrypted.</param>
        /// <param name="pOffset">The offset into the Plaintext to be encrypted.</param>
        /// <param name="inputLength">The Total number of data bytes to be encrypted.</param>
        /// <param name="cipherText">The Ciphertext resulting from the encryption.</param>
        /// <param name="tag">The Authentication tag.</param>
        /// <param name="tagLen">The length of Authentication tag.</param>
        /// <returns>
        /// The number of bytes encrypted.
        /// </returns>
        public int  GCMEncrypt(byte[] inputIV, int IVLen, byte[] add, int aLen, byte[] p, int pOffset, int inputLength, byte[] cipherText, byte[] tag, int tagLen)
        {
  
            return 0;
        }

         /// <summary>
        /// Decrypts the specified region of the input byte array and copies the decrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputIV">The initial vector.</param>
        /// <param name="IVLen">The offset into the input byte array from which to begin using data.</param>
        /// <param name="add">The Additional authenticated data.</param>
        /// <param name="aLen">The Length of the additional data.</param>
        /// <param name="p">The Plaintext to be decrypted.</param>
        /// <param name="pOffset">The offset into the Plaintext to be decrypted.</param>
        /// <param name="inputLength">The Total number of data bytes to be decrypted.</param>
        /// <param name="cipherText">The Ciphertext resulting from the encryption.</param>
        /// <param name="tag">The Authentication tag.</param>
        /// <param name="tagLen">The length of Authentication tag.</param>
        /// <returns>
        /// The number of bytes decrypted.
        /// </returns>
        public int GCMDecrypt(byte[] inputIV, int IVLen, byte[] add, int aLen, byte[] p, int pOffset, int inputLength, byte[] cipherText, byte[] tag, int tagLen)
        {
  
            return 0;
        }
    }

}
