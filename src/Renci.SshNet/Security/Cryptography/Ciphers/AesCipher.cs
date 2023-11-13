using System;
using System.Security.Cryptography;

using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Custom AES Cipher Mode, follows System.Security.Cryptography.CipherMode.
    /// </summary>
    public enum AesCipherMode
    {
        /// <summary>CBC Mode.</summary>
        CBC = 1,

        /// <summary>ECB Mode.</summary>
        ECB = 2,

        /// <summary>OFB Mode.</summary>
        OFB = 3,

        /// <summary>CFB Mode.</summary>
        CFB = 4,

        /// <summary>CTS Mode.</summary>
        CTS = 5,

        /// <summary>CTR Mode.</summary>
        CTR = 6
    }

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
        /// <param name="mode">The mode.</param>
        /// <param name="iv">The IV.</param>
        /// <param name="pkcs7Padding">Enable PKCS7 padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
        public AesCipher(byte[] key, byte[] iv, AesCipherMode mode, bool pkcs7Padding = false)
            : base(key, 16, mode: null, padding: null)
        {
            if (mode == AesCipherMode.OFB)
            {
                // OFB is not supported on modern .NET
                _impl = new BlockImpl(key, new OfbCipherMode(iv), pkcs7Padding ? new PKCS7Padding() : null);
            }
#if !NET6_0_OR_GREATER
            else if (mode == AesCipherMode.CFB)
            {
                // CFB not supported on NetStandard 2.1
                _impl = new BlockImpl(key, new CfbCipherMode(iv), pkcs7Padding ? new PKCS7Padding() : null);
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
                    (System.Security.Cryptography.CipherMode) mode,
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

        /// <summary>
        /// Dispose the instance.
        /// </summary>
        /// <param name="disposing">Set to True to dispose of resouces.</param>
        public void Dispose(bool disposing)
        {
            if (disposing && _impl is IDisposable disposableImpl)
            {
                disposableImpl.Dispose();
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
