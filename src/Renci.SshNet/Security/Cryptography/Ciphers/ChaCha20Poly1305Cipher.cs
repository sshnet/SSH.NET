#if NET6_0_OR_GREATER
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// ChaCha20Poly1305 cipher implementation.
    /// <see href="https://datatracker.ietf.org/doc/html/draft-josefsson-ssh-chacha20-poly1305-openssh-00"/>.
    /// </summary>
    internal sealed class ChaCha20Poly1305Cipher : SymmetricCipher, IDisposable
    {
        private readonly ChaCha20Poly1305 _chacha20poly1305;

        private readonly byte[] _sequenceNumber = new byte[12];
        private ChaCha20Cipher _aadCipher;

        /// <summary>
        /// Gets the minimun block size.
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
        /// Poly1305 [Poly1305], also by Daniel Bernstein, is a one-time Carter-
        /// Wegman MAC that computes a 128 bit integrity tag given a message
        /// <see href="https://datatracker.ietf.org/doc/html/draft-josefsson-ssh-chacha20-poly1305-openssh-00#section-1"/>.
        /// </summary>
        public override int TagSize
        {
            get
            {
                return 16;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChaCha20Poly1305Cipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public ChaCha20Poly1305Cipher(byte[] key)
            : base(key)
        {
            _chacha20poly1305 = new ChaCha20Poly1305(key.AsSpan(0, 32));
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
            var packetLengthField = _aadCipher.Encrypt(input, offset, 4);
            var plainText = new ReadOnlySpan<byte>(input, offset + 4, length - 4);

            var output = new byte[length + TagSize];
            Array.Copy(packetLengthField, output, 4);
            var cipherText = new Span<byte>(output, 4, length - 4);
            var tag = new Span<byte>(output, length, TagSize);

            _chacha20poly1305.Encrypt(nonce: _sequenceNumber, plainText, cipherText, tag, associatedData: packetLengthField);

            return output;
        }

        public override void SetSequenceNumber(uint sequenceNumber)
        {
            BinaryPrimitives.WriteUInt64BigEndian(_sequenceNumber, sequenceNumber);
            _aadCipher = new ChaCha20Cipher(Key.Take(32, 32), nonce: _sequenceNumber);
        }

        /// <summary>
        /// Decrypts the first block which is packet length field.
        /// </summary>
        /// <param name="input">The encrpted packet length field.</param>
        /// <returns>The decrypted packet length field.</returns>
        public override byte[] Decrypt(byte[] input)
        {
            return _aadCipher.Decrypt(input);
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

            _chacha20poly1305.Decrypt(nonce: _sequenceNumber, cipherText, tag, plainText, associatedData: packetLengthField);

            return output;
        }

        /// <summary>
        /// Dispose the instance.
        /// </summary>
        /// <param name="disposing">Set to True to dispose of resouces.</param>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _chacha20poly1305.Dispose();
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
