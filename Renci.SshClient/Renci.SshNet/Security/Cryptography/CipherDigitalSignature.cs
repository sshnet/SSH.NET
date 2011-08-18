using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements digital signature where where asymmetric cipher is used,
    /// </summary>
    public class CipherDigitalSignature : DigitalSignature
    {
        private HashAlgorithm _hash;

        private AsymmetricCipher _cipher;

        private ObjectIdentifier _oid;

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherDigitalSignature"/> class.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <param name="cipher">The cipher.</param>
        public CipherDigitalSignature(HashAlgorithm hash, ObjectIdentifier oid, AsymmetricCipher cipher)
        {
            if (hash == null)
                throw new ArgumentNullException("hash");

            if (cipher == null)
                throw new ArgumentNullException("cipher");

            this._hash = hash;
            this._cipher = cipher;
            this._oid = oid;
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="signature">The signature.</param>
        /// <returns></returns>
        public override bool Verify(byte[] input, byte[] signature)
        {
            var sig = this._cipher.Decrypt(signature);

            //  TODO:   Ensure that only 1 or 2 types are supported
            var position = 1;
            while (position < sig.Length && sig[position] != 0)
                position++;
            position++;


            var sig1 = new byte[sig.Length - position];

            Array.Copy(sig, position, sig1, 0, sig1.Length);

            var hashData = this.Hash(input);

            var expected = DerEncode(hashData);

            if (expected.Length != sig1.Length)
                return false;

            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != sig1[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public override byte[] Sign(byte[] input)
        {
            //  Calculate hash value
            var hashData = this.Hash(input);

            //  Calculate DER string
            var derEncodedHash = DerEncode(hashData);

            //  Calculate signature
            var rsaInputBlockSize = new byte[255];
            rsaInputBlockSize[0] = 0x01;
            for (int i = 1; i < rsaInputBlockSize.Length - derEncodedHash.Length - 1; i++)
            {
                rsaInputBlockSize[i] = 0xFF;
            }

            Array.Copy(derEncodedHash, 0, rsaInputBlockSize, rsaInputBlockSize.Length - derEncodedHash.Length, derEncodedHash.Length);

            return this._cipher.Encrypt(rsaInputBlockSize).TrimLeadingZero().ToArray();
        }

        /// <summary>
        /// Hashes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        protected byte[] Hash(byte[] input)
        {
            return this._hash.ComputeHash(input);
        }

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
