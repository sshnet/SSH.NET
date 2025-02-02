using System;
using System.Security.Cryptography;

#if !NET
using Org.BouncyCastle.Crypto.Paddings;

using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
#endif

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements 3DES cipher algorithm.
    /// </summary>
    public sealed partial class TripleDesCipher : BlockCipher, IDisposable
    {
#if NET
        private readonly BclImpl _impl;
#else
        private readonly BlockCipher _impl;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleDesCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The IV.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="pkcs7Padding">Enable PKCS7 padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public TripleDesCipher(byte[] key, byte[] iv, System.Security.Cryptography.CipherMode mode, bool pkcs7Padding)
            : base(key, 8, mode: null, padding: null)
        {
#if !NET
            if (mode == System.Security.Cryptography.CipherMode.CFB)
            {
                // CFB8 not supported on .NET Framework, but supported on .NET
                // see https://github.com/microsoft/referencesource/blob/51cf7850defa8a17d815b4700b67116e3fa283c2/mscorlib/system/security/cryptography/tripledescryptoserviceprovider.cs#L76-L78
                // see https://github.com/dotnet/runtime/blob/e7d837da5b1aacd9325a8b8f2214cfaf4d3f0ff6/src/libraries/System.Security.Cryptography/src/System/Security/Cryptography/TripleDesImplementation.cs#L229-L236
                _impl = new BlockImpl(key, new CfbCipherMode(iv), pkcs7Padding ? new Pkcs7Padding() : null);
            }
            else
#endif
            {
                _impl = new BclImpl(key, iv, mode, pkcs7Padding ? PaddingMode.PKCS7 : PaddingMode.None);
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
#if NET
            _impl.Dispose();
#else
            if (_impl is IDisposable disposableImpl)
            {
                disposableImpl.Dispose();
            }
#endif
        }
    }
}
