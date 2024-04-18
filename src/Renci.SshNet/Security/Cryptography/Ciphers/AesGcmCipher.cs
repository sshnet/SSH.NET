﻿#if NET6_0_OR_GREATER
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// AES GCM cipher implementation.
    /// <see href="https://datatracker.ietf.org/doc/html/rfc5647"/>.
    /// </summary>
    internal sealed class AesGcmCipher : SymmetricCipher, IDisposable
    {
        private readonly byte[] _iv;
        private readonly AesGcm _aesGcm;

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
#if NET8_0_OR_GREATER
            _aesGcm = new AesGcm(key, TagSize);
#else
            _aesGcm = new AesGcm(key);
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
            var packetLengthField = new ReadOnlySpan<byte>(input, offset, 4);
            var plainText = new ReadOnlySpan<byte>(input, offset + 4, length - 4);

            var output = new byte[length + TagSize];
            packetLengthField.CopyTo(output);
            var cipherText = new Span<byte>(output, 4, length - 4);
            var tag = new Span<byte>(output, length, TagSize);

            _aesGcm.Encrypt(nonce: _iv, plainText, cipherText, tag, associatedData: packetLengthField);

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

            var packetLengthField = new ReadOnlySpan<byte>(input, 4, 4);
            var cipherText = new ReadOnlySpan<byte>(input, offset, length);
            var tag = new ReadOnlySpan<byte>(input, offset + length, TagSize);

            var output = new byte[length];
            var plainText = new Span<byte>(output);

            _aesGcm.Decrypt(nonce: _iv, cipherText, tag, plainText, associatedData: packetLengthField);

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
