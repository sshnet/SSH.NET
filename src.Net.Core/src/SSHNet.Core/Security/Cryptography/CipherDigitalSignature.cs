using System;
using System.Linq;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements digital signature where where asymmetric cipher is used,
    /// </summary>
    public abstract class CipherDigitalSignature : DigitalSignature
    {
        private readonly AsymmetricCipher _cipher;

        private readonly ObjectIdentifier _oid;

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherDigitalSignature"/> class.
        /// </summary>
        /// <param name="oid">The object identifier.</param>
        /// <param name="cipher">The cipher.</param>
        public CipherDigitalSignature(ObjectIdentifier oid, AsymmetricCipher cipher)
        {
            if (cipher == null)
                throw new ArgumentNullException("cipher");

            this._cipher = cipher;
            this._oid = oid;
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>
        ///   <c>True</c> if signature was successfully verified; otherwise <c>false</c>.
        /// </returns>
        public override bool Verify(byte[] input, byte[] signature)
        {
            var encryptedSignature = this._cipher.Decrypt(signature);
            var hashData = this.Hash(input);
            var expected = DerEncode(hashData);
            return expected.SequenceEqual(encryptedSignature);
        }

        /// <summary>
        /// Creates the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// Signed input data.
        /// </returns>
        public override byte[] Sign(byte[] input)
        {
            //  Calculate hash value
            var hashData = this.Hash(input);

            //  Calculate DER string
            var derEncodedHash = DerEncode(hashData);

            return this._cipher.Encrypt(derEncodedHash).TrimLeadingZero().ToArray();
        }

        /// <summary>
        /// Hashes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Hashed data.</returns>
        protected abstract byte[] Hash(byte[] input);

        /// <summary>
        /// Encodes hash using DER.
        /// </summary>
        /// <param name="hashData">The hash data.</param>
        /// <returns>DER Encoded byte array</returns>
        protected byte[] DerEncode(byte[] hashData)
        {
            var data = new DerData();

            var alg = new DerData();
            alg.Write(this._oid);
            alg.WriteNull();

            data.Write(alg);
            data.Write(hashData);

            return data.Encode();
        }
    }
}
