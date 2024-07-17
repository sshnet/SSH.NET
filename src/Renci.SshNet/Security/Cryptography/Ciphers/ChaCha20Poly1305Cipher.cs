using System;
using System.Buffers.Binary;
using System.Diagnostics;

using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// ChaCha20Poly1305 cipher implementation.
    /// <see href="https://datatracker.ietf.org/doc/html/draft-josefsson-ssh-chacha20-poly1305-openssh-00"/>.
    /// </summary>
    internal sealed class ChaCha20Poly1305Cipher : SymmetricCipher
    {
        private readonly byte[] _sequenceNumber = new byte[12];
        private readonly ChaCha7539Engine _aadCipher = new ChaCha7539Engine();
        private readonly ChaCha7539Engine _cipher = new ChaCha7539Engine();
        private readonly Poly1305 _mac = new Poly1305();

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

            _aadCipher.ProcessBytes(input, offset, 4, output, 0);
            _cipher.ProcessBytes(input, offset + 4, length - 4, output, 4);

            _mac.BlockUpdate(output, 0, length);
            _ = _mac.DoFinal(output, length);

            return output;
        }

        /// <summary>
        /// Decrypts the first block which is packet length field.
        /// </summary>
        /// <param name="input">The encrypted packet length field.</param>
        /// <returns>The decrypted packet length field.</returns>
        public override byte[] Decrypt(byte[] input)
        {
            var output = new byte[input.Length];
            _aadCipher.ProcessBytes(input, 0, input.Length, output, 0);

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

            var tag = new byte[TagSize];
            _mac.BlockUpdate(input, offset - 4, length + 4);
            _ = _mac.DoFinal(tag, 0);
            if (!Arrays.FixedTimeEquals(TagSize, tag, 0, input, offset + length))
            {
                throw new SshConnectionException("MAC error", DisconnectReason.MacError);
            }

            var output = new byte[length];
            _cipher.ProcessBytes(input, offset, length, output, 0);

            return output;
        }

        internal override void SetSequenceNumber(uint sequenceNumber)
        {
            BinaryPrimitives.WriteUInt64BigEndian(_sequenceNumber.AsSpan(4), sequenceNumber);

            // ChaCha20 encryption and decryption is completely
            // symmetrical, so the 'forEncryption' is
            // irrelevant. (Like 90% of stream ciphers)
            _aadCipher.Init(forEncryption: true, new ParametersWithIV(new KeyParameter(Key, 32, 32), _sequenceNumber));
            _cipher.Init(forEncryption: true, new ParametersWithIV(new KeyParameter(Key, 0, 32), _sequenceNumber));

            var polyKeyBytes = new byte[32];
            _cipher.ProcessBytes(new byte[32], 0, 32, polyKeyBytes, 0);
            _mac.Init(new KeyParameter(polyKeyBytes));
        }
    }
}
