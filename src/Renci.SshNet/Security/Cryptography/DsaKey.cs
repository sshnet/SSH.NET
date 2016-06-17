using System;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains DSA private and public key
    /// </summary>
    public class DsaKey : Key, IDisposable
    {
        /// <summary>
        /// Gets the P.
        /// </summary>
        public BigInteger P
        {
            get
            {
                return _privateKey[0];
            }
        }

        /// <summary>
        /// Gets the Q.
        /// </summary>
        public BigInteger Q
        {
            get
            {
                return _privateKey[1];
            }
        }

        /// <summary>
        /// Gets the G.
        /// </summary>
        public BigInteger G
        {
            get
            {
                return _privateKey[2];
            }
        }

        /// <summary>
        /// Gets public key Y.
        /// </summary>
        public BigInteger Y
        {
            get
            {
                return _privateKey[3];
            }
        }

        /// <summary>
        /// Gets private key X.
        /// </summary>
        public BigInteger X
        {
            get
            {
                return _privateKey[4];
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
                return P.BitLength;
            }
        }

        private DsaDigitalSignature _digitalSignature;
        /// <summary>
        /// Gets the digital signature.
        /// </summary>
        protected override DigitalSignature DigitalSignature
        {
            get
            {
                if (_digitalSignature == null)
                {
                    _digitalSignature = new DsaDigitalSignature(this);
                }
                return _digitalSignature;
            }
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
                return new[] { P, Q, G, Y };
            }
            set
            {
                if (value.Length != 4)
                    throw new InvalidOperationException("Invalid public key.");

                _privateKey = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaKey"/> class.
        /// </summary>
        public DsaKey()
        {
            _privateKey = new BigInteger[5];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaKey"/> class.
        /// </summary>
        /// <param name="data">DER encoded private key data.</param>
        public DsaKey(byte[] data)
            : base(data)
        {
            if (_privateKey.Length != 5)
                throw new InvalidOperationException("Invalid private key.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaKey" /> class.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        /// <param name="g">The g.</param>
        /// <param name="y">The y.</param>
        /// <param name="x">The x.</param>
        public DsaKey(BigInteger p, BigInteger q, BigInteger g, BigInteger y, BigInteger x)
        {
            _privateKey = new BigInteger[5];
            _privateKey[0] = p;
            _privateKey[1] = q;
            _privateKey[2] = g;
            _privateKey[3] = y;
            _privateKey[4] = x;
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
                var digitalSignature = _digitalSignature;
                if (digitalSignature != null)
                {
                    digitalSignature.Dispose();
                    _digitalSignature = null;
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="DsaKey"/> is reclaimed by garbage collection.
        /// </summary>
        ~DsaKey()
        {
            Dispose(false);
        }

        #endregion
    }
}
