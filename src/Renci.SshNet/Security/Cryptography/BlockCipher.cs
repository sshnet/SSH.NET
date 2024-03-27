using System;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for block cipher implementations.
    /// </summary>
    public abstract class BlockCipher : SymmetricCipher
    {
        private readonly CipherMode _mode;

        private readonly CipherPadding _padding;

        /// <summary>
        /// Gets the size of the block in bytes.
        /// </summary>
        /// <value>
        /// The size of the block in bytes.
        /// </value>
        private readonly byte _blockSize;

        /// <summary>
        /// Gets the minimum data size.
        /// </summary>
        /// <value>
        /// The minimum data size.
        /// </value>
        public override byte MinimumSize
        {
            get { return BlockSize; }
        }

        /// <summary>
        /// Gets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        public byte BlockSize
        {
            get
            {
                return _blockSize;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="mode">Cipher mode.</param>
        /// <param name="padding">Cipher padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        protected BlockCipher(byte[] key, byte blockSize, CipherMode mode, CipherPadding padding)
            : base(key)
        {
            _blockSize = blockSize;
            _mode = mode;
            _padding = padding;

            _mode?.Init(this);
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
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            if (length % _blockSize > 0)
            {
                if (_padding is null)
                {
                    throw new ArgumentException("data");
                }

                var paddingLength = _blockSize - (length % _blockSize);
                input = _padding.Pad(input, offset, length, paddingLength);
                length += paddingLength;
                offset = 0;
            }

            var output = new byte[length];
            var writtenBytes = 0;

            for (var i = 0; i < length / _blockSize; i++)
            {
                if (_mode is null)
                {
                    writtenBytes += EncryptBlock(input, offset + (i * _blockSize), _blockSize, output, i * _blockSize);
                }
                else
                {
                    writtenBytes += _mode.EncryptBlock(input, offset + (i * _blockSize), _blockSize, output, i * _blockSize);
                }
            }

            if (writtenBytes < length)
            {
                throw new InvalidOperationException("Encryption error.");
            }

            return output;
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
        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            if (length % _blockSize > 0)
            {
                if (_padding is null)
                {
                    throw new ArgumentException("data");
                }

                input = _padding.Pad(_blockSize, input, offset, length);
                offset = 0;
                length = input.Length;
            }

            var output = new byte[length];

            var writtenBytes = 0;
            for (var i = 0; i < length / _blockSize; i++)
            {
                if (_mode is null)
                {
                    writtenBytes += DecryptBlock(input, offset + (i * _blockSize), _blockSize, output, i * _blockSize);
                }
                else
                {
                    writtenBytes += _mode.DecryptBlock(input, offset + (i * _blockSize), _blockSize, output, i * _blockSize);
                }
            }

            if (writtenBytes < length)
            {
                throw new InvalidOperationException("Encryption error.");
            }

            return output;
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
        public abstract int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);
    }
}
