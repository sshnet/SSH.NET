#nullable enable
using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements RSA digital signature algorithm.
    /// </summary>
    public class RsaDigitalSignature : DigitalSignature, IDisposable
    {
        private readonly RsaKey _key;
        private readonly HashAlgorithmName _hashAlgorithmName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaDigitalSignature"/> class with the SHA-1 hash algorithm.
        /// </summary>
        /// <param name="rsaKey">The RSA key.</param>
        public RsaDigitalSignature(RsaKey rsaKey)
            : this(rsaKey, HashAlgorithmName.SHA1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaDigitalSignature"/> class.
        /// </summary>
        /// <param name="rsaKey">The RSA key.</param>
        /// <param name="hashAlgorithmName">The hash algorithm to use in the digital signature.</param>
        public RsaDigitalSignature(RsaKey rsaKey, HashAlgorithmName hashAlgorithmName)
        {
            _key = rsaKey;
            _hashAlgorithmName = hashAlgorithmName;
        }

        /// <inheritdoc/>
        public override bool Verify(byte[] input, byte[] signature)
        {
            return _key.RSA.VerifyData(input, signature, _hashAlgorithmName, RSASignaturePadding.Pkcs1);
        }

        /// <inheritdoc/>
        public override byte[] Sign(byte[] input)
        {
            return _key.RSA.SignData(input, _hashAlgorithmName, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
