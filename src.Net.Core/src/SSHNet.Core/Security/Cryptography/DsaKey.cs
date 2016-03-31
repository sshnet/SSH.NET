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
                return this._privateKey[0];
            }
        }

        /// <summary>
        /// Gets the Q.
        /// </summary>
        public BigInteger Q
        {
            get
            {
                return this._privateKey[1];
            }
        }

        /// <summary>
        /// Gets the G.
        /// </summary>
        public BigInteger G
        {
            get
            {
                return this._privateKey[2];
            }
        }

        /// <summary>
        /// Gets public key Y.
        /// </summary>
        public BigInteger Y
        {
            get
            {
                return this._privateKey[3];
            }
        }

        /// <summary>
        /// Gets private key X.
        /// </summary>
        public BigInteger X
        {
            get
            {
                return this._privateKey[4];
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
                return this.P.BitLength;
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
                if (this._digitalSignature == null)
                {
                    this._digitalSignature = new DsaDigitalSignature(this);
                }
                return this._digitalSignature;
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
                return new BigInteger[] { this.P, this.Q, this.G, this.Y };
            }
            set
            {
                if (value.Length != 4)
                    throw new InvalidOperationException("Invalid public key.");

                this._privateKey = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaKey"/> class.
        /// </summary>
        public DsaKey()
        {
            this._privateKey = new BigInteger[5];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaKey"/> class.
        /// </summary>
        /// <param name="data">DER encoded private key data.</param>
        public DsaKey(byte[] data)
            : base(data)
        {
            if (this._privateKey.Length != 5)
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
            this._privateKey = new BigInteger[5];
            this._privateKey[0] = p;
            this._privateKey[1] = q;
            this._privateKey[2] = g;
            this._privateKey[3] = y;
            this._privateKey[4] = x;
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged ResourceMessages.
                if (disposing)
                {
                    // Dispose managed ResourceMessages.
                    if (this._digitalSignature != null)
                    {
                        this._digitalSignature.Dispose();
                        this._digitalSignature = null;
                    }
                }

                // Note disposing has been done.
                this._isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SshCommand"/> is reclaimed by garbage collection.
        /// </summary>
        ~DsaKey()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
