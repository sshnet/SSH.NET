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
            var paddingLength = _blockSize - (length % _blockSize);

            if (_padding is not null)
            {
                input = _padding.Pad(input, offset, length, paddingLength);
                length = input.Length;
                offset = 0;
            }
            else if (paddingLength != _blockSize)
            {
                throw new ArgumentException("The specified plaintext size is not valid for the padding and block size.");
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
        /// Decrypts the specified data.
        /// </summary>
        /// <param name="input">The data.</param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        public override byte[] Decrypt(byte[] input)
        {
            return Decrypt(input, 0, input.Length);
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
            var originalLength = length;

            if (length % _blockSize != 0)
            {
                // Resize the input to allow decrypting non-block-sized inputs
                // in a block-by-block manner.
                var tmpInput = new byte[length + _blockSize - (length % _blockSize)];

                Buffer.BlockCopy(input, offset, tmpInput, 0, length);

                input = tmpInput;
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

            // Do a dance around padding to satisfy the cases where _padding
            // has been specified but the plaintext is not actually padded,
            // and where a non-block-sized input has been given so we have
            // increased the length above.

            if (length > originalLength)
            {
                // The input was a non-block-sized length, so it was not padded.
                // Just resize back to the original length and return.

                Array.Resize(ref output, originalLength);
            }
            else if (_padding is not null)
            {
                var unpaddedLength = _padding.GetUnpaddedLength(output);

                if (unpaddedLength < length)
                {
                    // _padding has been specified and the plaintext does have
                    // valid padding. Remove it.

                    Array.Resize(ref output, unpaddedLength);
                }
                else
                {
                    // _padding has been specified but the plaintext is not padded.
                    // Do nothing.
                }
            }

            return output;
        }
    }
}
