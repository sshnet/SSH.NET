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
        private readonly byte[] _iv;
        private readonly int _aadLength;
        private readonly KeyParameter _aadKeyParameter;
        private readonly KeyParameter _keyParameter;
        private readonly ChaCha7539Engine _aadCipher;
        private readonly ChaCha7539Engine _cipher;
        private readonly Poly1305 _mac;

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
        /// <param name="aadLength">The length of additional associated data.</param>
        public ChaCha20Poly1305Cipher(byte[] key, int aadLength)
            : base(key)
        {
            _iv = new byte[12];
            _aadLength = aadLength;

            _keyParameter = new KeyParameter(key, 0, 32);
            _cipher = new ChaCha7539Engine();

            if (aadLength > 0)
            {
                _aadKeyParameter = new KeyParameter(key, 32, 32);
                _aadCipher = new ChaCha7539Engine();
            }

            _mac = new Poly1305();
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
        /// <returns>
        /// The encrypted data with below format:
        ///   <code>
        ///   [----Cipher AAD----][----Cipher Text----][----TAG----]
        ///   </code>
        /// </returns>
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            _aadCipher?.Init(forEncryption: true, new ParametersWithIV(_aadKeyParameter, _iv));
            _cipher.Init(forEncryption: true, new ParametersWithIV(_keyParameter, _iv));

            var keyStream = new byte[64];
            _cipher.ProcessBytes(keyStream, 0, keyStream.Length, keyStream, 0);
            _mac.Init(new KeyParameter(keyStream, 0, 32));

            var output = new byte[length + TagSize];

            _aadCipher?.ProcessBytes(input, offset, _aadLength, output, 0);
            _cipher.ProcessBytes(input, offset + _aadLength, length - _aadLength, output, _aadLength);

            _mac.BlockUpdate(output, 0, length);
            _ = _mac.DoFinal(output, length);

            return output;
        }

        /// <summary>
        /// Decrypts the AAD.
        /// </summary>
        /// <param name="input">The encrypted AAD.</param>
        /// <returns>The decrypted AAD.</returns>
        public override byte[] Decrypt(byte[] input)
        {
            Debug.Assert(_aadCipher != null, "The aadCipher must not be null");

            _aadCipher.Init(forEncryption: false, new ParametersWithIV(_aadKeyParameter, _iv));

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
        ///   [----][----Cipher AAD----(offset)][----Cipher Text----(length)][----TAG----]
        ///   </code>
        /// </param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin decrypting and authenticating.</param>
        /// <param name="length">The number of bytes to decrypt and authenticate from <paramref name="input"/>.</param>
        /// <returns>
        /// The decrypted data with below format:
        /// <code>
        ///   [----Plain Text----]
        /// </code>
        /// </returns>
        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            Debug.Assert(offset >= _aadLength, "The offset must be greater than or equals to aad length");

            _cipher.Init(forEncryption: false, new ParametersWithIV(_keyParameter, _iv));

            var keyStream = new byte[64];
            _cipher.ProcessBytes(keyStream, 0, keyStream.Length, keyStream, 0);
            _mac.Init(new KeyParameter(keyStream, 0, 32));

            var tag = new byte[TagSize];
            _mac.BlockUpdate(input, offset - _aadLength, length + _aadLength);
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
            BinaryPrimitives.WriteUInt64BigEndian(_iv.AsSpan(4), sequenceNumber);
        }
    }
}
