using System;
using Renci.SshNet.Common;
using csp = System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Base class for cipher mode implementations.
    /// </summary>
    public abstract class CipherMode
    {
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1306 // Field names should begin with lower-case letter
        /// <summary>
        /// Gets the cipher.
        /// </summary>
        protected BlockCipher Cipher;

        /// <summary>
        /// Gets the IV vector.
        /// </summary>
        internal byte[] IV;

        /// <summary>
        /// Holds block size of the cipher.
        /// </summary>
        protected int _blockSize;
#pragma warning restore SA1306 // Field names should begin with lower-case letter
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherMode"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        protected CipherMode(byte[] iv)
        {
            if (iv.Length < 16)
                throw new ArgumentException("Invalid AES IV length");

            IV = iv;
        }

        /// <summary>
        /// Initializes the specified cipher mode.
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        internal void Init(BlockCipher cipher)
        {
            Cipher = cipher;
            _blockSize = cipher.BlockSize;
            IV = IV.Take(_blockSize);
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
        public abstract int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

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
        public virtual int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            // By default, use the same EncryptBlock() function
            // Modes that require a different implementation (non-symmetric) can override this function (CBC/CFB)
            return EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

#if FEATURE_AES_CSP
        // CryptoServiceProvider acceleration using AES-NI if supported
        internal csp.ICryptoTransform aesDecryptor;
        internal csp.ICryptoTransform aesEncryptor;

        // set to false when CSP is not available; falls back to legacy code
        internal bool isCspAvailable = true;

        // corresponding CSP cipher mode
        internal csp.CipherMode cspMode = csp.CipherMode.ECB;

        /// <summary>
        /// If true, performs decryption using aesEncryptor
        /// </summary>
        protected bool cspDecryptAsEncrypt = true;

        /// <summary>
        /// Initializes the specified cipher mode using CryptoServiceProvider acceleration
        /// </summary>
        /// <param name="cipher">The cipher.</param>
        /// <param name="csp">The cryptoServiceProvider instance</param>
        internal void Init(BlockCipher cipher, csp.AesCryptoServiceProvider csp)
        {
            Init(cipher);
            try
            {
                aesDecryptor = csp.CreateDecryptor(csp.Key, IV);
                aesEncryptor = csp.CreateEncryptor(csp.Key, IV);
            }
            catch
            {
                // OFB/CFB might not be available on some versions of Windows - fallback to legacy code
                isCspAvailable = false;
            }
        }

        /// <summary>
        /// Encrypts the specified region of the input byte array using AesCryptoServiceProvider
        /// </summary>
        /// <param name="data">The input data to encrypt.</param>
        /// <param name="offset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="output">The output to which to write encrypted data.</param>
        /// <returns>The number of bytes encrypted</returns>
        public virtual int EncryptWithCSP(byte[] data, int offset, byte[] output)
        {
            return aesEncryptor.TransformBlock(data, offset, output.Length, output, 0);
        }

        /// <summary>
        /// Decrypts the specified region of the input byte array using AesCryptoServiceProvider
        /// </summary>
        /// <param name="data">The input data to decrypt.</param>
        /// <param name="offset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="output">The output to which to write decrypted data.</param>
        /// <returns>The number of bytes decrypted</returns>
        public virtual int DecryptWithCSP(byte[] data, int offset, byte[] output)
        {
            if (cspDecryptAsEncrypt)
                return EncryptWithCSP(data, offset, output);

            return aesDecryptor.TransformBlock(data, offset, output.Length, output, 0);
        }
#endif
    }
}
