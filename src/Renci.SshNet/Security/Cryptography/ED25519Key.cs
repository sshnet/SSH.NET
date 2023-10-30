using System;

using Renci.SshNet.Common;
using Renci.SshNet.Security.Chaos.NaCl;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains ED25519 private and public key.
    /// </summary>
    public class ED25519Key : Key, IDisposable
    {
        private ED25519DigitalSignature _digitalSignature;

        private byte[] _publicKey = new byte[Ed25519.PublicKeySizeInBytes];
#pragma warning disable IDE1006 // Naming Styles
        private readonly byte[] privateKey = new byte[Ed25519.ExpandedPrivateKeySizeInBytes];
#pragma warning restore IDE1006 // Naming Styles
        private bool _isDisposed;

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
                return new BigInteger[] { _publicKey.ToBigInteger2() };
            }
            set
            {
                _publicKey = value[0].ToByteArray().Reverse().TrimLeadingZeros().Pad(Ed25519.PublicKeySizeInBytes);
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
                return PublicKey.Length * 8;
            }
        }

        /// <summary>
        /// Gets the digital signature.
        /// </summary>
        protected internal override DigitalSignature DigitalSignature
        {
            get
            {
                _digitalSignature ??= new ED25519DigitalSignature(this);
                return _digitalSignature;
            }
        }

        /// <summary>
        /// Gets the PublicKey Bytes.
        /// </summary>
        public byte[] PublicKey
        {
            get
            {
                return _publicKey;
            }
        }

        /// <summary>
        /// Gets the PrivateKey Bytes.
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
        public ED25519Key(byte[] pk)
        {
            _publicKey = pk.TrimLeadingZeros().Pad(Ed25519.PublicKeySizeInBytes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ED25519Key"/> class.
        /// </summary>
        /// <param name="pk">pk data.</param>
        /// <param name="sk">sk data.</param>
        public ED25519Key(byte[] pk, byte[] sk)
        {
            _publicKey = pk.TrimLeadingZeros().Pad(Ed25519.PublicKeySizeInBytes);
            var seed = new byte[Ed25519.PrivateKeySeedSizeInBytes];
            Buffer.BlockCopy(sk, 0, seed, 0, seed.Length);
            Ed25519.KeyPairFromSeed(out _publicKey, out privateKey, seed);
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
        /// Releases unmanaged and - optionally - managed resources
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
