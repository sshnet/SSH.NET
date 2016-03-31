using System;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains RSA private and public key
    /// </summary>
    public class RsaKey : Key, IDisposable
    {
        /// <summary>
        /// Gets the modulus.
        /// </summary>
        public BigInteger Modulus
        {
            get
            {
                return this._privateKey[0];
            }
        }

        /// <summary>
        /// Gets the exponent.
        /// </summary>
        public BigInteger Exponent
        {
            get
            {
                return this._privateKey[1];
            }
        }

        /// <summary>
        /// Gets the D.
        /// </summary>
        public BigInteger D
        {
            get
            {
                if (this._privateKey.Length > 2)
                    return this._privateKey[2];
                return BigInteger.Zero;
            }
        }

        /// <summary>
        /// Gets the P.
        /// </summary>
        public BigInteger P
        {
            get
            {
                if (this._privateKey.Length > 3)
                    return this._privateKey[3];
                return BigInteger.Zero;
            }
        }

        /// <summary>
        /// Gets the Q.
        /// </summary>
        public BigInteger Q
        {
            get
            {
                if (this._privateKey.Length > 4)
                    return this._privateKey[4];
                return BigInteger.Zero;
            }
        }

        /// <summary>
        /// Gets the DP.
        /// </summary>
        public BigInteger DP
        {
            get
            {
                if (this._privateKey.Length > 5)
                    return this._privateKey[5];
                return BigInteger.Zero;
            }
        }

        /// <summary>
        /// Gets the DQ.
        /// </summary>
        public BigInteger DQ
        {
            get
            {
                if (this._privateKey.Length > 6)
                    return this._privateKey[6];
                return BigInteger.Zero;
            }
        }

        /// <summary>
        /// Gets the inverse Q.
        /// </summary>
        public BigInteger InverseQ
        {
            get
            {
                if (this._privateKey.Length > 7)
                    return this._privateKey[7];
                return BigInteger.Zero;
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
                return this.Modulus.BitLength;
            }
        }

        private RsaDigitalSignature _digitalSignature;
        /// <summary>
        /// Gets the digital signature.
        /// </summary>
        protected override DigitalSignature DigitalSignature
        {
            get
            {
                if (this._digitalSignature == null)
                {
                    this._digitalSignature = new RsaDigitalSignature(this);
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
                return new BigInteger[] { this.Exponent, this.Modulus };
            }
            set
            {
                if (value.Length != 2)
                    throw new InvalidOperationException("Invalid private key.");

                this._privateKey = new BigInteger[] { value[1], value[0] };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaKey"/> class.
        /// </summary>
        public RsaKey()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaKey"/> class.
        /// </summary>
        /// <param name="data">DER encoded private key data.</param>
        public RsaKey(byte[] data)
            : base(data)
        {
            if (this._privateKey.Length != 8)
                throw new InvalidOperationException("Invalid private key.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaKey"/> class.
        /// </summary>
        /// <param name="modulus">The modulus.</param>
        /// <param name="exponent">The exponent.</param>
        /// <param name="d">The d.</param>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        /// <param name="inverseQ">The inverse Q.</param>
        public RsaKey(BigInteger modulus, BigInteger exponent, BigInteger d, BigInteger p, BigInteger q, BigInteger inverseQ)
        {
            this._privateKey = new BigInteger[8];
            this._privateKey[0] = modulus;
            this._privateKey[1] = exponent;
            this._privateKey[2] = d;
            this._privateKey[3] = p;
            this._privateKey[4] = q;
            this._privateKey[5] = PrimeExponent(d, p);
            this._privateKey[6] = PrimeExponent(d, q);
            this._privateKey[7] = inverseQ;
        }

        private static BigInteger PrimeExponent(BigInteger privateExponent, BigInteger prime)
        {
            BigInteger pe = prime - new BigInteger(1);
            return privateExponent % pe;
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
        ~RsaKey()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
