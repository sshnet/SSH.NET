#nullable enable
using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements DSA digital signature algorithm.
    /// </summary>
    public class DsaDigitalSignature : DigitalSignature, IDisposable
    {
        private readonly DsaKey _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaDigitalSignature" /> class.
        /// </summary>
        /// <param name="key">The DSA key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public DsaDigitalSignature(DsaKey key)
        {
            ThrowHelper.ThrowIfNull(key);

            _key = key;
        }

        /// <inheritdoc/>
        public override bool Verify(byte[] input, byte[] signature)
        {
#if NETSTANDARD2_1_OR_GREATER || NET
            return _key.DSA.VerifyData(input, signature, HashAlgorithmName.SHA1);
#else
            // VerifyData does not exist on netstandard2.0.
            // It does exist on net462, but in order to keep the path tested,
            // use it on netfx as well.
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(input);
                return _key.DSA.VerifySignature(hash, signature);
            }
#endif
        }

        /// <summary>
        /// Creates the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// Signed input data.
        /// </returns>
        /// <exception cref="SshException">Invalid DSA key.</exception>
        public override byte[] Sign(byte[] input)
        {
#if NETSTANDARD2_1_OR_GREATER || NET
            return _key.DSA.SignData(input, HashAlgorithmName.SHA1);
#else
            // SignData does not exist on netstandard2.0.
            // It does exist on net462, but in order to keep the path tested,
            // use it on netfx as well.
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(input);
                return _key.DSA.CreateSignature(hash);
            }
#endif
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
