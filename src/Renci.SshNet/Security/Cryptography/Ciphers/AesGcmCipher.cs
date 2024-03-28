#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// AES GCM cipher implementation.
    /// </summary>
    public sealed class AesGcmCipher : AeadCipher, IDisposable
    {
        private readonly AesGcm _aesGcm;

        /// <summary>
        /// Initializes a new instance of the <see cref="AesGcmCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The IV.</param>
        public AesGcmCipher(byte[] key, byte[] iv)
            : base(key, iv, nonceSize: 12, tagSize: 16)
        {
#if NET8_0_OR_GREATER
            _aesGcm = new AesGcm(key, TagSize);
#else
            _aesGcm = new AesGcm(key);
#endif
        }

        /// <inheritdoc/>
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            // [outbound sequence][packet length field][padding length field sz][payload][random paddings]
            // [-----4 bytes-----][----4 bytes(offset)][-------------------Plain Text--------------------]
            var associatedData = new ReadOnlySpan<byte>(input, offset - 4, 4);
            var plainText = new ReadOnlySpan<byte>(input, offset, length);

            var cipherText = new byte[length];
            var tag = new byte[TagSize];

            _aesGcm.Encrypt(IV, plainText, cipherText, tag, associatedData);

            var result = new byte[length + TagSize];
            Buffer.BlockCopy(cipherText, 0, result, 0, length);
            Buffer.BlockCopy(tag, 0, result, length, TagSize);

            IncrementCounter();

            return result;
        }

        /// <inheritdoc/>
        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            // [inbound sequence][packet length field][padding length field sz][payload][random paddings][Authenticated TAG]
            // [-----4 bytes----][----4 bytes(offset)][------------------Cipher Text--------------------][-------TAG-------]
            var associatedData = new ReadOnlySpan<byte>(input, offset - 4, 4);
            var cipherText = new ReadOnlySpan<byte>(input, offset, length);
            var tag = new ReadOnlySpan<byte>(input, offset + length, TagSize);

            var plainText = new byte[length];

            _aesGcm.Decrypt(IV, cipherText, tag, plainText, associatedData);

            IncrementCounter();

            return plainText;
        }

        private void IncrementCounter()
        {
            var invocationCounter = IV.Take(4, 8);
            var count = Pack.BigEndianToUInt64(invocationCounter) + 1;
            invocationCounter = Pack.UInt64ToBigEndian(count);
            Buffer.BlockCopy(invocationCounter, 0, IV, 4, 8);
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
