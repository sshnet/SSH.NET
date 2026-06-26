#nullable enable
using System;
using System.Buffers.Binary;
using System.Diagnostics;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// AES GCM cipher implementation.
    /// <see href="https://datatracker.ietf.org/doc/html/rfc5647"/>.
    /// </summary>
    internal sealed partial class AesGcmCipher : SymmetricCipher, IDisposable
    {
        private const int TagSizeInBytes = 16;
        private readonly byte[] _iv;
        private readonly int _aadLength;
#if !NETSTANDARD
        private readonly Impl _impl;
#else
        private readonly BouncyCastleImpl _impl;
#endif

        /// <summary>
        /// Gets the minimum block size.
        /// The reader is reminded that SSH requires that the data to be
        /// encrypted MUST be padded out to a multiple of the block size
        /// (16-octets for AES-GCM).
        /// <see href="https://datatracker.ietf.org/doc/html/rfc5647#section-7.1"/>.
        /// </summary>
        public override byte MinimumSize
        {
            get
            {
                return 16;
            }
        }

        /// <summary>
        /// Gets the tag size in bytes.
        /// Both AEAD_AES_128_GCM and AEAD_AES_256_GCM produce a 16-octet
        /// Authentication Tag
        /// <see href="https://datatracker.ietf.org/doc/html/rfc5647#section-6.3"/>.
        /// </summary>
        public override int TagSize
        {
            get
            {
                return TagSizeInBytes;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AesGcmCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The IV.</param>
        /// <param name="aadLength">The length of additional associated data.</param>
        public AesGcmCipher(byte[] key, byte[] iv, int aadLength)
            : base(key)
        {
            // SSH AES-GCM requires a 12-octet Initial IV
            _iv = iv.Take(12);
            _aadLength = aadLength;
#if !NETSTANDARD
            if (System.Security.Cryptography.AesGcm.IsSupported)
            {
                _impl = new BclImpl(key, _iv);
            }
            else
#endif
            {
                _impl = new BouncyCastleImpl(key, _iv);
            }
        }

        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            var output = new byte[length + TagSizeInBytes];

            var bytesWritten = Encrypt(input, offset, length, output, 0);

            Debug.Assert(bytesWritten == output.Length);

            return output;
        }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">
        /// The input data with below format:
        ///   <code>
        ///   [----(offset)][----AAD----][----Plain Text----(length)]
        ///   </code>
        /// </param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="input"/>.</param>
        /// <param name="output">The output buffer to write to.</param>
        /// <param name="outputOffset">The zero-based offset in <paramref name="output"/> at which to write encrypted output.</param>
        /// <remarks>
        /// The output data is written with the below format:
        ///   <code>
        ///   [----AAD----][----Cipher Text----][----TAG----]
        ///   </code>
        /// </remarks>
        public override int Encrypt(byte[] input, int offset, int length, byte[] output, int outputOffset)
        {
            input.AsSpan(offset, _aadLength).CopyTo(output.AsSpan(outputOffset));

            _impl.Encrypt(
                input,
                plainTextOffset: offset + _aadLength,
                plainTextLength: length - _aadLength,
                associatedDataOffset: offset,
                associatedDataLength: _aadLength,
                output,
                outputOffset + _aadLength);

            IncrementCounter();

            return length + TagSizeInBytes;
        }

        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            var output = new byte[length];

            var bytesWritten = Decrypt(input, offset, length, output, 0);

            Debug.Assert(bytesWritten == length);

            return output;
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">
        /// The input data with below format:
        ///   <code>
        ///   [----][----AAD----(offset)][----Cipher Text----(length)][----TAG----]
        ///   </code>
        /// </param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin decrypting and authenticating.</param>
        /// <param name="length">The number of bytes to decrypt and authenticate from <paramref name="input"/>.</param>
        /// <param name="output">The buffer to which to write the decrypted bytes.</param>
        /// <param name="outputOffset">The zero-based offset in <paramref name="output"/> at which to write the decrypted bytes.</param>
        /// <returns>The number of plaintext bytes written to <paramref name="output"/>.</returns>
        public override int Decrypt(byte[] input, int offset, int length, byte[] output, int outputOffset)
        {
            Debug.Assert(offset >= _aadLength, "The offset must be greater than or equal to aad length");

            _impl.Decrypt(
                input,
                cipherTextOffset: offset,
                cipherTextLength: length,
                associatedDataOffset: offset - _aadLength,
                associatedDataLength: _aadLength,
                output,
                outputOffset);

            IncrementCounter();

            return length;
        }

        /// <summary>
        /// With AES-GCM, the 12-octet IV is broken into two fields: a 4-octet
        /// fixed field and an 8 - octet invocation counter field.The invocation
        /// field is treated as a 64 - bit integer and is incremented after each
        /// invocation of AES - GCM to process a binary packet.
        /// <see href="https://datatracker.ietf.org/doc/html/rfc5647#section-7.1"/>.
        /// </summary>
        private void IncrementCounter()
        {
            var invocationCounter = new Span<byte>(_iv, 4, 8);
            var count = BinaryPrimitives.ReadUInt64BigEndian(invocationCounter);
            BinaryPrimitives.WriteUInt64BigEndian(invocationCounter, count + 1);
        }

        /// <summary>
        /// Dispose the instance.
        /// </summary>
        /// <param name="disposing">Set to True to dispose of resources.</param>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _impl.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private abstract class Impl : IDisposable
        {
            public abstract void Encrypt(byte[] input, int plainTextOffset, int plainTextLength, int associatedDataOffset, int associatedDataLength, byte[] output, int cipherTextOffset);

            public abstract void Decrypt(byte[] input, int cipherTextOffset, int cipherTextLength, int associatedDataOffset, int associatedDataLength, byte[] output, int plainTextOffset);

            protected virtual void Dispose(bool disposing)
            {
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
