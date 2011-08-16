using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Base class for asymmetric cipher algorithms
    /// </summary>
    public abstract class Key
    {
        /// <summary>
        /// Gets the key specific digital signature.
        /// </summary>
        protected abstract DigitalSignature DigitalSignature { get; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        /// <value>
        /// The public.
        /// </value>
        public abstract BigInteger[] Public { get; set; }

        /// <summary>
        /// Gets or sets the private key.
        /// </summary>
        /// <value>
        /// The private.
        /// </value>
        protected abstract BigInteger[] Private { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Key"/> class.
        /// </summary>
        /// <param name="data">DER encoded private key data.</param>
        public Key(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var der = new DerData(data);
            var version = der.ReadBigInteger();

            var keys = new List<BigInteger>();
            while (!der.IsEndOfData)
            {
                keys.Add(der.ReadBigInteger());
            }

            this.Private = keys.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Key"/> class.
        /// </summary>
        public Key()
            : base()
        {

        }

        /// <summary>
        /// Signs the specified data with the key.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <returns>Signed data.</returns>
        public byte[] Sign(byte[] data)
        {
            return this.DigitalSignature.Sign(data);
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="data">The data to verify.</param>
        /// <param name="signature">The signature to verify against.</param>
        /// <returns></returns>
        public bool VerifySignature(byte[] data, byte[] signature)
        {
            return this.DigitalSignature.Verify(data, signature);
        }
    }
}
