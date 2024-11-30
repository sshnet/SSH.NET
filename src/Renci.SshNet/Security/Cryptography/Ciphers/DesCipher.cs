using System;
using System.Security.Cryptography;

using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements DES cipher algorithm.
    /// </summary>
    public sealed partial class DesCipher : BlockCipher, IDisposable
    {
        private readonly BlockCipher _impl;

        /// <summary>
        /// Initializes a new instance of the <see cref="DesCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The IV.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="pkcs7Padding">Enable PKCS7 padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public DesCipher(byte[] key, byte[] iv, BlockCipherMode mode, bool pkcs7Padding)
            : base(key, 8, mode: null, padding: null)
        {
            if (mode == BlockCipherMode.CFB)
            {
                // BCL's DES CFB implementation is different with OpenSSL
                _impl = new BouncyCastleImpl(key, new CfbCipherMode(iv), pkcs7Padding ? new PKCS7Padding() : null);
            }
            else
            {
                _impl = new BclImpl(key, iv, (System.Security.Cryptography.CipherMode)mode, pkcs7Padding ? PaddingMode.PKCS7 : PaddingMode.None);
            }
        }

        /// <inheritdoc/>
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            return _impl.Encrypt(input, offset, length);
        }

        /// <inheritdoc/>
        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            return _impl.Decrypt(input, offset, length);
        }

        /// <inheritdoc/>
        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return _impl.EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <inheritdoc/>
        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return _impl.DecryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_impl is IDisposable disposableImpl)
            {
                disposableImpl.Dispose();
            }
        }
    }
}
