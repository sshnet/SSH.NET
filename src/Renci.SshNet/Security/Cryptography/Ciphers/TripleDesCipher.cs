using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements 3DES cipher algorithm.
    /// </summary>
    internal sealed class TripleDesCipher : Cipher, IDisposable
    {
        private readonly TripleDES _des;
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;

        public override byte MinimumSize
        {
            get
            {
                return 8;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleDesCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The IV.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="pkcs7Padding">Enable PKCS7 padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public TripleDesCipher(byte[] key, byte[] iv, CipherMode mode, bool pkcs7Padding = false)
            : base(key)
        {
            var des = TripleDES.Create();
            des.Key = key;
            des.IV = iv.Take(8);
            des.Mode = mode;
            des.Padding = pkcs7Padding ? PaddingMode.PKCS7 : PaddingMode.None;
            _des = des;
            _encryptor = _des.CreateEncryptor();
            _decryptor = _des.CreateDecryptor();
        }

        /// <inheritdoc/>
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            if (_des.Padding != PaddingMode.None)
            {
                return _encryptor.TransformFinalBlock(input, offset, length);
            }

            // Otherwise, (the most important case) assume this instance is
            // used for one direction of an SSH connection, whereby the
            // encrypted data in all packets are considered a single data
            // stream i.e. we do not want to reset the state between calls to Encrypt.
            var output = new byte[length];
            _ = _encryptor.TransformBlock(input, offset, length, output, 0);

            return output;
        }

        /// <inheritdoc/>
        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            if (_des.Padding != PaddingMode.None)
            {
                // If padding has been specified, call TransformFinalBlock to apply
                // the padding and reset the state.
                return _decryptor.TransformFinalBlock(input, offset, length);
            }

            // Otherwise, (the most important case) assume this instance is
            // used for one direction of an SSH connection, whereby the
            // encrypted data in all packets are considered a single data
            // stream i.e. we do not want to reset the state between calls to Encrypt.
            var output = new byte[length];
            _ = _decryptor.TransformBlock(input, offset, length, output, 0);

            return output;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _des.Dispose();
            _encryptor.Dispose();
            _decryptor.Dispose();
        }
    }
}
