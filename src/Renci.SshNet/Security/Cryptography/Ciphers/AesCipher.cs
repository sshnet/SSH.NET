#nullable enable
using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// AES cipher implementation.
    /// </summary>
    internal sealed class AesCipher : Cipher, IDisposable
    {
        private readonly Aes _aes;
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;

        /// <inheritdoc/>
        public override byte MinimumSize
        {
            get
            {
                return 16;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AesCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The IV.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="pkcs7Padding">Enable PKCS7 padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
        public AesCipher(byte[] key, byte[] iv, CipherMode mode, bool pkcs7Padding = false)
            : base(key)
        {
            var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv.Take(16);
            aes.Mode = mode;
            aes.Padding = pkcs7Padding ? PaddingMode.PKCS7 : PaddingMode.None;
            _aes = aes;
            _encryptor = aes.CreateEncryptor();
            _decryptor = aes.CreateDecryptor();
        }

        /// <inheritdoc/>
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            return Transform(_encryptor, input, offset, length, output: null, 0, out _);
        }

        /// <inheritdoc/>
        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            return Transform(_decryptor, input, offset, length, output: null, 0, out _);
        }

        /// <inheritdoc/>
        public override int Decrypt(byte[] input, int offset, int length, byte[] output, int outputOffset)
        {
            _ = Transform(_decryptor, input, offset, length, output, outputOffset, out var bytesWritten);

            return bytesWritten;
        }

        private byte[] Transform(ICryptoTransform transform, byte[] input, int offset, int length, byte[]? output, int outputOffset, out int bytesWritten)
        {
            if (_aes.Padding != PaddingMode.None)
            {
                // If padding has been specified, call TransformFinalBlock to apply
                // the padding and reset the state.

                var finalBlock = transform.TransformFinalBlock(input, offset, length);

                if (output is not null)
                {
                    finalBlock.AsSpan().CopyTo(output.AsSpan(outputOffset));
                }

                bytesWritten = finalBlock.Length;

                return finalBlock;
            }

            // Otherwise, (the most important case) assume this instance is
            // used for one direction of an SSH connection, whereby the
            // encrypted data in all packets are considered a single data
            // stream i.e. we do not want to reset the state between calls to Decrypt.
            if (output is null)
            {
                output = new byte[length];

                bytesWritten = transform.TransformBlock(input, offset, length, output, outputOffset);

                // Manually unpad the output.
                Array.Resize(ref output, bytesWritten);
            }
            else
            {
                bytesWritten = transform.TransformBlock(input, offset, length, output, outputOffset);
            }

            return output;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _aes.Dispose();
            _encryptor.Dispose();
            _decryptor.Dispose();
        }
    }
}
