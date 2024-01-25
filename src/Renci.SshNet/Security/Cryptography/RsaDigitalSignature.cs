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
#if NET462
        private readonly HashAlgorithm _hash;
#else
        private readonly IncrementalHash _hash;
#endif

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
            : base(ObjectIdentifier.FromHashAlgorithmName(hashAlgorithmName), new RsaCipher(rsaKey))
        {
#if NET462
            _hash = CryptoConfig.CreateFromName(hashAlgorithmName.Name) as HashAlgorithm
                ?? throw new ArgumentException($"Could not create {nameof(HashAlgorithm)} from `{hashAlgorithmName}`.", nameof(hashAlgorithmName));
#else
            // CryptoConfig.CreateFromName is a somewhat legacy API and is incompatible with trimming.
            // Use IncrementalHash instead (which is also more modern and lighter-weight than HashAlgorithm).
            _hash = IncrementalHash.CreateHash(hashAlgorithmName);
#endif
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
#if NET462
            return _hash.ComputeHash(input);
#else
            _hash.AppendData(input);
            return _hash.GetHashAndReset();
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
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            _hash.Dispose();
        }

        #endregion
    }
}
