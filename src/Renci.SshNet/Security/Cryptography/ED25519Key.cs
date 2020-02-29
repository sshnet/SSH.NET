using System;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Chaos.NaCl;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains ED25519 private and public key
    /// </summary>
    public class ED25519Key : Key, IDisposable
    {
        private ED25519DigitalSignature _digitalSignature;

        private byte[] publicKey = new byte[Ed25519.PublicKeySizeInBytes];
        private byte[] privateKey = new byte[Ed25519.ExpandedPrivateKeySizeInBytes];

        /// <summary>
        /// Gets the Key String.
        /// </summary>
        public override string ToString()
        {
            return "ssh-ed25519";
        }

        /// <summary>
        /// Gets or sets the public.
        /// </summary>
        /// <value>
        /// The public.
        /// </value>
        public override BigInteger[] Public
        {
            get
            {
                return new BigInteger[] { publicKey.ToBigInteger() };
            }
            set
            {
                publicKey = value[0].ToByteArray().Reverse().TrimLeadingZeros().Pad(Ed25519.PublicKeySizeInBytes);
            }
        }

        /// <summary>
        /// Gets the length of the key.
        /// </summary>
        /// <value>
        /// The length of the key.
        /// </value>
        public override int KeyLength
        {
            get
            {
                return PublicKey.Length;
            }
        }

        /// <summary>
        /// Gets the digital signature.
        /// </summary>
        protected override DigitalSignature DigitalSignature
        {
            get
            {
                if (_digitalSignature == null)
                {
                    _digitalSignature = new ED25519DigitalSignature(this);
                }
                return _digitalSignature;
            }
        }

        /// <summary>
        /// Gets the PublicKey Bytes
        /// </summary>
        public byte[] PublicKey
        {
            get
            {
                return publicKey;
            }
        }

        /// <summary>
        /// Gets the PrivateKey Bytes
        /// </summary>
        public byte[] PrivateKey
        {
            get
            {
                return privateKey;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ED25519Key"/> class.
        /// </summary>
        public ED25519Key()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ED25519Key"/> class.
        /// </summary>
        /// <param name="pk">pk data.</param>
        /// <param name="sk">sk data.</param>
        public ED25519Key(byte[] pk, byte[] sk)
        {
            publicKey = pk.TrimLeadingZeros().Pad(Ed25519.PublicKeySizeInBytes);
            var seed = new byte[Ed25519.PrivateKeySeedSizeInBytes];
            Buffer.BlockCopy(sk, 0, seed, 0, seed.Length);
            Ed25519.KeyPairFromSeed(out publicKey, out privateKey, seed);
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="DsaKey"/> is reclaimed by garbage collection.
        /// </summary>
        ~ED25519Key()
        {
            Dispose(false);
        }

        #endregion
    }
}
