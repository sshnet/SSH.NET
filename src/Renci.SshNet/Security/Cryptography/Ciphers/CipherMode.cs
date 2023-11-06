using System;
using System.Globalization;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Base class for cipher mode implementations.
    /// </summary>
    public abstract class CipherMode : IDisposable
    {
        private readonly System.Security.Cryptography.CipherMode _aesMode;

        private Aes _aes;
        private ICryptoTransform _encryptor;
        private ICryptoTransform _decryptor;
        private CipherPadding _padding;

        /// <summary>
        /// Gets a value indicating whether to process arrays in one go using CNG provider
        /// Set to False to process arrays block by block.
        /// </summary>
        protected virtual bool SupportsMultipleBlocks
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the AES Encryptor instance.
        /// </summary>
        protected ICryptoTransform Encryptor
        {
            get
            {
                if (_encryptor == null || !_encryptor.CanReuseTransform)
                {
                    _encryptor = _aes.CreateEncryptor();
                }

                return _encryptor;
            }
        }

        /// <summary>
        /// Gets the AES Decryptor instance.
        /// </summary>
        protected ICryptoTransform Decryptor
        {
            get
            {
                if (_decryptor == null || !_decryptor.CanReuseTransform)
                {
                    _decryptor = _aes.CreateDecryptor();
                }

                return _decryptor;
            }
        }

        /// <summary>
        /// Gets the IV vector.
        /// </summary>
        protected internal byte[] IV { get; private set; }

        /// <summary>
        /// Gets the block size of the cipher.
        /// </summary>
        protected int BlockSize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherMode"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        /// <param name="aesMode">The AES mode.</param>
        protected CipherMode(byte[] iv, System.Security.Cryptography.CipherMode aesMode)
        {
            IV = iv;
            _aesMode = aesMode;
        }

        /// <summary>
        /// Initializes the specified cipher mode.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="blocksize">The block size.</param>
        /// <param name="padding">Enable PKCS7 padding.</param>
        internal void Init(byte[] key, int blocksize, CipherPadding padding)
        {
            BlockSize = blocksize;
            IV = IV.Take(BlockSize);
            _padding = padding;

            _aes = Aes.Create();
            _aes.BlockSize = BlockSize * 8;
            _aes.FeedbackSize = BlockSize * 8;
            _aes.Mode = _aesMode;
            _aes.Padding = padding is null ? PaddingMode.None : PaddingMode.PKCS7;
            _aes.Key = key;
            _aes.IV = IV;
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
        public virtual int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer.Length - inputOffset < BlockSize)
            {
                throw new ArgumentException("Invalid input buffer");
            }

            if (outputBuffer.Length - outputOffset < BlockSize)
            {
                throw new ArgumentException("Invalid output buffer");
            }

            if (inputCount != BlockSize)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "inputCount must be {0}.", BlockSize));
            }

            return Encryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
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
        public virtual int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer.Length - inputOffset < BlockSize)
            {
                throw new ArgumentException("Invalid input buffer");
            }

            if (outputBuffer.Length - outputOffset < BlockSize)
            {
                throw new ArgumentException("Invalid output buffer");
            }

            if (inputCount != BlockSize)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "inputCount must be {0}.", BlockSize));
            }

            return Decryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="input">The data.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The encrypted data.
        /// </returns>
        public virtual byte[] Encrypt(byte[] input, int offset, int length)
        {
            if (SupportsMultipleBlocks && Encryptor.CanTransformMultipleBlocks)
            {
                return Encryptor.TransformFinalBlock(input, offset, length);
            }

            return EncryptArray(input, offset, length);
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin decrypting.</param>
        /// <param name="length">The number of bytes to decrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        public virtual byte[] Decrypt(byte[] input, int offset, int length)
        {
            if (SupportsMultipleBlocks && Decryptor.CanTransformMultipleBlocks)
            {
                return Decryptor.TransformFinalBlock(input, offset, length);
            }

            return DecryptArray(input, offset, length);
        }

        // Encrypts the data using EncryptBlock() on each block.
        private byte[] EncryptArray(byte[] input, int offset, int length)
        {
            if (length % BlockSize > 0)
            {
                if (_padding is null)
                {
                    throw new ArgumentException("data");
                }

                var paddingLength = BlockSize - (length % BlockSize);
                input = _padding.Pad(input, offset, length, paddingLength);
                length += paddingLength;
                offset = 0;
            }

            var output = new byte[length];
            var writtenBytes = 0;

            for (var i = 0; i < length / BlockSize; i++)
            {
                writtenBytes += EncryptBlock(input, offset + (i * BlockSize), BlockSize, output, i * BlockSize);
            }

            if (writtenBytes < length)
            {
                throw new InvalidOperationException("Encryption error.");
            }

            return output;
        }

        // Decrypts the data using DecryptBlock() on each block.
        private byte[] DecryptArray(byte[] input, int offset, int length)
        {
            if (length % BlockSize > 0)
            {
                if (_padding is null)
                {
                    throw new ArgumentException("data");
                }

                input = _padding.Pad(BlockSize, input, offset, length);
                offset = 0;
                length = input.Length;
            }

            var output = new byte[length];

            var writtenBytes = 0;
            for (var i = 0; i < length / BlockSize; i++)
            {
                writtenBytes += DecryptBlock(input, offset + (i * BlockSize), BlockSize, output, i * BlockSize);
            }

            if (writtenBytes < length)
            {
                throw new InvalidOperationException("Encryption error.");
            }

            return output;
        }

        /// <summary>
        /// Dispose the instance.
        /// </summary>
        /// <param name="disposing">Set to True to dispose of resouces.</param>
        protected virtual void Dispose(bool disposing)
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
