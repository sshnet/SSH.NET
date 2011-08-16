using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains RSA private and public key
    /// </summary>
    public class RsaKey : Key
    {
        /// <summary>
        /// Gets the modulus.
        /// </summary>
        public BigInteger Modulus { get; private set; }

        /// <summary>
        /// Gets the exponent.
        /// </summary>
        public BigInteger Exponent { get; private set; }

        /// <summary>
        /// Gets the D.
        /// </summary>
        public BigInteger D { get; private set; }

        /// <summary>
        /// Gets the P.
        /// </summary>
        public BigInteger P { get; private set; }

        /// <summary>
        /// Gets the Q.
        /// </summary>
        public BigInteger Q { get; private set; }

        /// <summary>
        /// Gets the DP.
        /// </summary>
        public BigInteger DP { get; private set; }

        /// <summary>
        /// Gets the DQ.
        /// </summary>
        public BigInteger DQ { get; private set; }

        /// <summary>
        /// Gets the inverse Q.
        /// </summary>
        public BigInteger InverseQ { get; private set; }

        private DigitalSignature _digitalSignature;
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

                this.Exponent = value[0];
                this.Modulus = value[1];
            }
        }

        /// <summary>
        /// Gets or sets the private.
        /// </summary>
        /// <value>
        /// The private.
        /// </value>
        protected override BigInteger[] Private
        {
            get
            {
                return new BigInteger[] { this.Modulus, this.Exponent, this.D, this.P, this.Q, this.DP, this.DQ, this.InverseQ };
            }
            set
            {
                if (value.Length != 8)
                    throw new InvalidOperationException("Invalid private key.");

                this.Modulus = value[0];
                this.Exponent = value[1];
                this.D = value[2];
                this.P = value[3];
                this.Q = value[4];
                this.DP = value[5];
                this.DQ = value[6];
                this.InverseQ = value[7];
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

        }
    }
}
