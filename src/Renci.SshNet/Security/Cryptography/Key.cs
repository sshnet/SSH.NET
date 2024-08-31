using System.Numerics;

using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Base class for asymmetric cipher algorithms.
    /// </summary>
    public abstract class Key
    {
        /// <summary>
        /// Gets the default digital signature implementation for this key.
        /// </summary>
        protected internal abstract DigitalSignature DigitalSignature { get; }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <value>
        /// The public.
        /// </value>
        public abstract BigInteger[] Public { get; }

        /// <summary>
        /// Gets the length of the key in bits.
        /// </summary>
        /// <value>
        /// The bit-length of the key.
        /// </value>
        public abstract int KeyLength { get; }

        /// <summary>
        /// Gets or sets the key comment.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Signs the specified data with the key.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <returns>
        /// Signed data.
        /// </returns>
        public byte[] Sign(byte[] data)
        {
            return DigitalSignature.Sign(data);
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="data">The data to verify.</param>
        /// <param name="signature">The signature to verify against.</param>
        /// <returns><see langword="true"/> is signature was successfully verifies; otherwise <see langword="false"/>.</returns>
        public bool VerifySignature(byte[] data, byte[] signature)
        {
            return DigitalSignature.Verify(data, signature);
        }
    }
}
