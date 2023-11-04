using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// AES cipher implementation.
    /// </summary>
    public sealed class AesCipher : BlockCipher, IDisposable
    {
        private readonly Aes _aes;
        private ICryptoTransform _encryptor;
        private ICryptoTransform _decryptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AesCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="padding">The padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
        public AesCipher(byte[] key, CipherMode mode, CipherPadding padding)
            : base(key, 16, mode, padding)
        {
            var aes = Aes.Create();
            aes.Key = key;

            if (mode is not null)
            {
                aes.IV = mode.IV;
            }

#pragma warning disable CA5358 // Do not use unsafe cipher modes; this is the basis for other modes.
            aes.Mode = System.Security.Cryptography.CipherMode.ECB;
#pragma warning restore CA5358 // Do not use unsafe cipher modes
            aes.Padding = PaddingMode.None;
            _aes = aes;
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
        /// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is too short.</exception>
        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            _encryptor ??= _aes.CreateEncryptor();

            return _encryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
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
        /// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is too short.</exception>
        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            _decryptor ??= _aes.CreateDecryptor();

            return _decryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _encryptor?.Dispose();
                _encryptor = null;

                _decryptor?.Dispose();
                _decryptor = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
