using System;
using System.Security.Cryptography;

using Org.BouncyCastle.Crypto.Paddings;

using Renci.SshNet.Security.Cryptography.Ciphers.Modes;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// AES cipher implementation.
    /// </summary>
    public sealed partial class AesCipher : BlockCipher, IDisposable
    {
        private readonly BlockCipher _impl;

        /// <summary>
        /// Initializes a new instance of the <see cref="AesCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The IV.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="pkcs7Padding">Enable PKCS7 padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
        public AesCipher(byte[] key, byte[] iv, AesCipherMode mode, bool pkcs7Padding = false)
            : base(key, 16, mode: null, padding: null)
        {
            if (mode == AesCipherMode.OFB)
            {
                // OFB is not supported on modern .NET
                _impl = new BlockImpl(key, new OfbCipherMode(iv), pkcs7Padding ? new Pkcs7Padding() : null);
            }
#if !NET
            else if (mode == AesCipherMode.CFB)
            {
                // CFB not supported on NetStandard 2.1
                _impl = new BlockImpl(key, new CfbCipherMode(iv), pkcs7Padding ? new Pkcs7Padding() : null);
            }
#endif
            else if (mode == AesCipherMode.CTR)
            {
                // CTR not supported by the BCL, use an optimized implementation
                _impl = new CtrImpl(key, iv);
            }
            else
            {
                _impl = new BclImpl(
                    key,
                    iv,
                    (System.Security.Cryptography.CipherMode)mode,
                    pkcs7Padding ? PaddingMode.PKCS7 : PaddingMode.None);
            }
        }

        /// <inheritdoc/>
        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return _impl.EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <inheritdoc/>
        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return _impl.EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
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
        public void Dispose()
        {
            if (_impl is IDisposable disposableImpl)
            {
                disposableImpl.Dispose();
            }
        }
    }
}
