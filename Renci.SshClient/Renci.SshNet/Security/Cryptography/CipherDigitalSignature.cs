using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements digital signature where where asymmetric cipher is used,
    /// </summary>
    public class CipherDigitalSignature : DigitalSignature
    {
        private HashAlgorithm _hash;

        private AsymmetricCipher _cipher;

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherDigitalSignature"/> class.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <param name="cipher">The cipher.</param>
        public CipherDigitalSignature(HashAlgorithm hash, AsymmetricCipher cipher)
        {
            if (hash == null)
                throw new ArgumentNullException("hash");

            if (cipher == null)
                throw new ArgumentNullException("cipher");

            this._hash = hash;
            this._cipher = cipher;
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

            if (expected.Count != sig1.Length)
                return false;

            for (int i = 0; i < expected.Count; i++)
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

            //  Resolve algorithm identifier
            var dd = DerEncode(hashData);

            //  Calculate signature
            var rsaInputBlockSize = new byte[255];
            rsaInputBlockSize[0] = 0x01;
            for (int i = 1; i < rsaInputBlockSize.Length - dd.Count - 1; i++)
            {
                rsaInputBlockSize[i] = 0xFF;
            }

            Array.Copy(dd.ToArray(), 0, rsaInputBlockSize, rsaInputBlockSize.Length - dd.Count, dd.Count);

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

        protected static List<byte> DerEncode(byte[] hashData)
        {
            //  TODO:   Replace with DER Encoding
            //  TODO:   Replace with algorithm code
            var algorithm = new byte[] { 6, 5, 43, 14, 3, 2, 26 };
            var algorithmParams = new byte[] { 5, 0 };

            var dd = new List<byte>(algorithm);
            dd.AddRange(algorithmParams);
            dd.Insert(0, (byte)dd.Count);
            dd.Insert(0, 48);

            dd.Add(4);
            dd.Add((byte)hashData.Length);
            dd.AddRange(hashData);

            dd.Insert(0, (byte)dd.Count);
            dd.Insert(0, 48);
            return dd;
        }
    }
}
