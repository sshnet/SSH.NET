using System;

using Org.BouncyCastle.Math.EC.Rfc8032;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements ECDSA digital signature algorithm.
    /// </summary>
    public class ED25519DigitalSignature : DigitalSignature, IDisposable
    {
        private readonly ED25519Key _key;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ED25519DigitalSignature" /> class.
        /// </summary>
        /// <param name="key">The ED25519Key key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public ED25519DigitalSignature(ED25519Key key)
        {
            ThrowHelper.ThrowIfNull(key);

            _key = key;
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>
        /// <see langword="true"/> if signature was successfully verified; otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">Invalid signature.</exception>
        public override bool Verify(byte[] input, byte[] signature)
        {
            return Ed25519.Verify(signature, 0, _key.PublicKey, 0, input, 0, input.Length);
        }

        /// <summary>
        /// Creates the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// Signed input data.
        /// </returns>
        /// <exception cref="SshException">Invalid ED25519Key key.</exception>
        public override byte[] Sign(byte[] input)
        {
            var signature = new byte[Ed25519.SignatureSize];
            Ed25519.Sign(_key.PrivateKey, 0, _key.PublicKey, 0, input, 0, input.Length, signature, 0);
            return signature;
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
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _isDisposed = true;
            }
        }
    }
}
