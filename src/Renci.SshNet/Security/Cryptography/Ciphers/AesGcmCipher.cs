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
        private const int PacketLengthFieldLength = 4;
        private readonly byte[] _iv;
#if NET6_0_OR_GREATER
        private readonly Impl _impl;

#else
        private readonly BouncyCastleImpl _impl;
#endif

        /// <summary>
        /// Gets the minimun block size.
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
            // SSH AES-GCM requires a 12-octet Initial IV
            _iv = iv.Take(12);
#if NET6_0_OR_GREATER
            if (System.Security.Cryptography.AesGcm.IsSupported)
            {
                _impl = new BclImpl(key, TagSize);
            }
            else
            {
                _impl = new BouncyCastleImpl(key, TagSize);
            }
#else
            _impl = new BouncyCastleImpl(key, TagSize);
#endif
        }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">
        /// The input data with below format:
        ///   <code>
        ///   [outbound sequence field][packet length field][padding length field sz][payload][random paddings]
        ///   [----4 bytes----(offset)][------4 bytes------][----------------Plain Text---------------(length)]
        ///   </code>
        /// </param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The encrypted data with below format:
        ///   <code>
        ///   [packet length field][padding length field sz][payload][random paddings][Authenticated TAG]
        ///   [------4 bytes------][------------------Cipher Text--------------------][-------TAG-------]
        ///   </code>
        /// </returns>
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            var output = new byte[length + TagSize];
            Buffer.BlockCopy(input, offset, output, 0, PacketLengthFieldLength);

            _impl.Encrypt(
                nonce: _iv,
                input,
                plainTextOffset: offset + PacketLengthFieldLength,
                plainTextLength: length - PacketLengthFieldLength,
                associatedDataOffset: offset,
                associatedDataLength: PacketLengthFieldLength,
                output,
                cipherTextOffset: PacketLengthFieldLength);

            IncrementCounter();

            return output;
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">
        /// The input data with below format:
        ///   <code>
        ///   [inbound sequence field][packet length field][padding length field sz][payload][random paddings][Authenticated TAG]
        ///   [--------4 bytes-------][--4 bytes--(offset)][--------------Cipher Text----------------(length)][-------TAG-------]
        ///   </code>
        /// </param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin decrypting and authenticating.</param>
        /// <param name="length">The number of bytes to decrypt and authenticate from <paramref name="input"/>.</param>
        /// <returns>
        /// The decrypted data with below format:
        /// <code>
        ///   [padding length field sz][payload][random paddings]
        ///   [--------------------Plain Text-------------------]
        /// </code>
        /// </returns>
        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            Debug.Assert(offset == 8, "The offset must be 8");

            var output = new byte[length];

            _impl.Decrypt(
                nonce: _iv,
                input,
                cipherTextOffset: offset,
                cipherTextLength: length,
                associatedDataOffset: offset - PacketLengthFieldLength,
                associatedDataLength: PacketLengthFieldLength,
                output,
                plainTextOffset: 0);

            IncrementCounter();

            return output;
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
        /// <param name="disposing">Set to True to dispose of resouces.</param>
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
            public abstract void Encrypt(byte[] nonce, byte[] input, int plainTextOffset, int plainTextLength, int associatedDataOffset, int associatedDataLength, byte[] output, int cipherTextOffset);

            public abstract void Decrypt(byte[] nonce, byte[] input, int cipherTextOffset, int cipherTextLength, int associatedDataOffset, int associatedDataLength, byte[] output, int plainTextOffset);

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
