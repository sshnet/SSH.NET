using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements RSA digital signature algorithm.
    /// </summary>
    public class RsaDigitalSignature : CipherDigitalSignature, IDisposable
    {
        private readonly HashAlgorithmName _hashAlgorithmName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaDigitalSignature"/> class with the SHA-1 hash algorithm.
        /// </summary>
        /// <param name="rsaKey">The RSA key.</param>
        public RsaDigitalSignature(RsaKey rsaKey)
            : this(rsaKey, HashAlgorithmName.SHA1)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaDigitalSignature"/> class.
        /// </summary>
        /// <param name="rsaKey">The RSA key.</param>
        /// <param name="hashAlgorithmName">The hash algorithm to use in the digital signature.</param>
        public RsaDigitalSignature(RsaKey rsaKey, HashAlgorithmName hashAlgorithmName)
            : base(ObjectIdentifier.FromHashAlgorithmName(hashAlgorithmName), new RsaCipher(rsaKey))
        {
            _hashAlgorithmName = hashAlgorithmName;
        }

        /// <summary>
        /// Hashes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// Hashed data.
        /// </returns>
        protected override byte[] Hash(byte[] input)
        {
#if !NET462
            using var hash = IncrementalHash.CreateHash(_hashAlgorithmName);
            hash.AppendData(input);
            return hash.GetHashAndReset();
#else
            using var hash = CryptoConfig.CreateFromName(_hashAlgorithmName.Name) as HashAlgorithm
                ?? throw new InvalidOperationException($"Could not create {nameof(HashAlgorithm)} from `{_hashAlgorithmName}`.");

            return hash.ComputeHash(input);
#endif
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        { }

        #endregion
    }
}
