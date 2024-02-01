﻿using System;

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
        private bool _isDisposed;

        /// <summary>
        /// Gets the name of the key.
        /// </summary>
        /// <returns>
        /// The name of the key.
        /// </returns>
        public override string ToString()
        {
            return "ssh-ed25519";
        }

        /// <summary>
        /// Gets the Ed25519 public key.
        /// </summary>
        /// <value>
        /// An array with <see cref="PublicKey"/> encoded at index 0.
        /// </value>
        public override BigInteger[] Public
        {
            get
            {
                return new BigInteger[] { PublicKey.ToBigInteger2() };
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
        public byte[] PublicKey { get; }

        /// <summary>
        /// Gets the PrivateKey Bytes.
        /// </summary>
        public byte[] PrivateKey { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ED25519Key"/> class.
        /// </summary>
        /// <param name="publicKeyData">The encoded public key data.</param>
        public ED25519Key(SshKeyData publicKeyData)
        {
            if (publicKeyData is null)
            {
                throw new ArgumentNullException(nameof(publicKeyData));
            }

            if (publicKeyData.Name != "ssh-ed25519" || publicKeyData.Keys.Length != 1)
            {
                throw new ArgumentException($"Invalid Ed25519 public key data ({publicKeyData.Name}, {publicKeyData.Keys.Length}).", nameof(publicKeyData));
            }

            PublicKey = publicKeyData.Keys[0].ToByteArray().Reverse().TrimLeadingZeros().Pad(Ed25519.PublicKeySizeInBytes);
            PrivateKey = new byte[Ed25519.ExpandedPrivateKeySizeInBytes];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ED25519Key"/> class.
        /// </summary>
        /// <param name="privateKeyData">
        /// The private key data <c>k || ENC(A)</c> as described in RFC 8032.
        /// </param>
        public ED25519Key(byte[] privateKeyData)
        {
            var seed = new byte[Ed25519.PrivateKeySeedSizeInBytes];
            Buffer.BlockCopy(privateKeyData, 0, seed, 0, seed.Length);
            Ed25519.KeyPairFromSeed(out var publicKey, out var privateKey, seed);
            PublicKey = publicKey;
            PrivateKey = privateKey;
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

        /// <summary>
        /// Finalizes an instance of the <see cref="ED25519Key"/> class.
        /// </summary>
        ~ED25519Key()
        {
            Dispose(disposing: false);
        }
    }
}
