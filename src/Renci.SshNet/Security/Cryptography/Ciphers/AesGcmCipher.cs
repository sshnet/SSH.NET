#if NET6_0_OR_GREATER
using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// AES GCM cipher implementation.
    /// <see href="https://datatracker.ietf.org/doc/html/rfc5647"/>.
    /// </summary>
    public sealed class AesGcmCipher : SymmetricCipher, IDisposable
    {
        private readonly byte[] _nonce;
        private readonly AesGcm _aesGcm;

        /// <inheritdoc/>
        public override byte MinimumSize
        {
            get
            {
                return 16;
            }
        }

        /// <inheritdoc/>
        public override int TagSize
        {
            get
            {
                return 16;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AesGcmCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The IV.</param>
        public AesGcmCipher(byte[] key, byte[] iv)
            : base(key)
        {
            _nonce = iv.Take(12);
#if NET8_0_OR_GREATER
            _aesGcm = new AesGcm(key, TagSize);
#else
            _aesGcm = new AesGcm(key);
#endif
        }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The packet length field + cipher text + tag.
        /// </returns>
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            // [outbound sequence field][packet length field][padding length field sz][payload][random paddings]
            // [----4 bytes----(offset)][------4 bytes------][----------------Plain Text---------------(length)]
            var packetLengthField = new ReadOnlySpan<byte>(input, offset, 4);
            var plainText = new ReadOnlySpan<byte>(input, offset + 4, length - 4);

            var output = new byte[length + TagSize];
            packetLengthField.CopyTo(output);
            var cipherText = new Span<byte>(output, 4, length - 4);
            var tag = new Span<byte>(output, length, TagSize);

            _aesGcm.Encrypt(_nonce, plainText, cipherText, tag, packetLengthField);

            IncrementCounter();

            return output;
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin decrypting and authenticating.</param>
        /// <param name="length">The number of bytes to decrypt and authenticate from <paramref name="input"/>.</param>
        /// <returns>
        /// The packet length field + plain text.
        /// </returns>
        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            // [inbound sequence field][packet length field][padding length field sz][payload][random paddings][Authenticated TAG]
            // [----4 bytes---(offset)][------4 bytes------][------------------Cipher Text--------------------][---TAG---(length)]
            var packetLengthField = new ReadOnlySpan<byte>(input, offset, 4);
            var cipherText = new ReadOnlySpan<byte>(input, offset + 4, length - 4 - TagSize);
            var tag = new ReadOnlySpan<byte>(input, offset + length - TagSize, TagSize);

            var output = new byte[length - TagSize];
            packetLengthField.CopyTo(output);
            var plainText = new Span<byte>(output, 4, length - 4 - TagSize);

            _aesGcm.Decrypt(_nonce, cipherText, tag, plainText, packetLengthField);

            IncrementCounter();

            return output;
        }

        private void IncrementCounter()
        {
            var invocationCounter = new Span<byte>(_nonce, 4, 8);
            var count = BinaryPrimitives.ReadUInt64BigEndian(invocationCounter);
            BinaryPrimitives.WriteUInt64BigEndian(invocationCounter, count + 1);
        }

        /// <summary>
        /// Dispose the instance.
        /// </summary>
        /// <param name="disposing">Set to True to dispose of resouces.</param>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _aesGcm.Dispose();
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
#endif
