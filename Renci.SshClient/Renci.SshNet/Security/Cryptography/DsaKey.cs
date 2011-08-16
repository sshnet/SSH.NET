using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains DSA private and public key
    /// </summary>
    public class DsaKey : Key
    {
        /// <summary>
        /// Gets public key Y.
        /// </summary>
        public BigInteger Y { get; private set; }

        /// <summary>
        /// Gets private key X.
        /// </summary>
        public BigInteger X { get; private set; }

        /// <summary>
        /// Gets the G.
        /// </summary>
        public BigInteger G { get; private set; }

        /// <summary>
        /// Gets the Q.
        /// </summary>
        public BigInteger Q { get; private set; }

        /// <summary>
        /// Gets the P.
        /// </summary>
        public BigInteger P { get; private set; }

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

                this.P = value[0];
                this.Q = value[1];
                this.G = value[2];
                this.Y = value[3];
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
                //return new BigInteger[] { this.P, this.Q, this.G, this.Y, this.X };
                return new BigInteger[] { this.P, this.Q, this.G, this.X };
            }
            set
            {
                if (value.Length != 5)
                    throw new InvalidOperationException("Invalid private key.");

                this.P = value[0];
                this.Q = value[1];
                this.G = value[2];
                this.Y = value[3];
                this.X = value[4];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaKey"/> class.
        /// </summary>
        public DsaKey()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaKey"/> class.
        /// </summary>
        /// <param name="data">DER encoded private key data.</param>
        public DsaKey(byte[] data)
            : base(data)
        {

        }
    }
}
