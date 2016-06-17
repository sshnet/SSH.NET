using System;
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
        protected CipherDigitalSignature(ObjectIdentifier oid, AsymmetricCipher cipher)
        {
            if (cipher == null)
                throw new ArgumentNullException("cipher");

            _cipher = cipher;
            _oid = oid;
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
            var encryptedSignature = _cipher.Decrypt(signature);
            var hashData = Hash(input);
            var expected = DerEncode(hashData);
            return expected.IsEqualTo(encryptedSignature);
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
            var hashData = Hash(input);

            //  Calculate DER string
            var derEncodedHash = DerEncode(hashData);

            return _cipher.Encrypt(derEncodedHash).TrimLeadingZeros();
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
            var alg = new DerData();
            alg.Write(_oid);
            alg.WriteNull();

            var data = new DerData();
            data.Write(alg);
            data.Write(hashData);
            return data.Encode();
        }
    }
}
