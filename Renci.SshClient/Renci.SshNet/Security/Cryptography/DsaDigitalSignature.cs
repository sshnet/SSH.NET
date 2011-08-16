using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Common;
using System.Globalization;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements DSA digital signature algorithm.
    /// </summary>
    public class DsaDigitalSignature : DigitalSignature
    {
        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();
        
        private HashAlgorithm _hash;

        private DsaKey _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaDigitalSignature"/> class.
        /// </summary>
        /// <param name="key">The DSA key.</param>
        public DsaDigitalSignature(DsaKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            this._key = key;

            this._hash = new SHA1Hash();
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="signature">The signature.</param>
        /// <returns></returns>
        public override bool Verify(byte[] input, byte[] signature)
        {
            var hashInput = this._hash.ComputeHash(input);

            BigInteger hm = new BigInteger(hashInput.Reverse().Concat(new byte[] { 0 }).ToArray());

            if (signature.Length != 40)
                throw new InvalidOperationException("Invalid signature.");

            //  Extract r and s numbers from the signature
            var rBytes = new byte[21];
            var sBytes = new byte[21];

            for (int i = 0, j = 20; i < 20; i++, j--)
            {
                rBytes[i] = signature[j - 1];
                sBytes[i] = signature[j + 20 - 1];
            }

            BigInteger r = new BigInteger(rBytes);
            BigInteger s = new BigInteger(sBytes);

            //  Reject the signature if 0 < r < q or 0 < s < q is not satisfied.
            if (r <= 0 || r >= this._key.Q)
                return false;

            if (s <= 0 || s >= this._key.Q)
                return false;

            //  Calculate w = s−1 mod q
            BigInteger w = BigInteger.ModInverse(s, this._key.Q);

            //  Calculate u1 = H(m)·w mod q
            BigInteger u1 = hm * w % this._key.Q;

            //  Calculate u2 = r * w mod q
            BigInteger u2 = r * w % this._key.Q;

            u1 = BigInteger.ModPow(this._key.G, u1, this._key.P);
            u2 = BigInteger.ModPow(this._key.Y, u2, this._key.P);

            //  Calculate v = ((g pow u1 * y pow u2) mod p) mod q
            BigInteger v = ((u1 * u2) % this._key.P) % this._key.Q;

            //  The signature is valid if v = r
            return v == r;
        }

        /// <summary>
        /// Creates the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public override byte[] Sign(byte[] input)
        {
            var hashInput = this._hash.ComputeHash(input);

            BigInteger m = new BigInteger(hashInput.Reverse().Concat(new byte[] { 0 }).ToArray());

            BigInteger s = BigInteger.Zero;
            BigInteger r = BigInteger.Zero;

            do
            {
                BigInteger k;

                do
                {
                    //  TODO:   Take random function to BigInteger

                    //  Generate a random per-message value k where 0 < k < q
                    do
                    {
                        var bytesArray = new byte[20];
                        _randomizer.GetBytes(bytesArray);

                        bytesArray[bytesArray.Length - 1] = (byte)(bytesArray[bytesArray.Length - 1] & 0x7F);   //  Ensure not a negative value
                        k = new BigInteger(bytesArray.Reverse().ToArray());
                    }
                    while (k <= 0 || k >= this._key.Q);

                    //  Calculate r = ((g pow k) mod p) mod q
                    r = BigInteger.ModPow(this._key.G, k, this._key.P) % this._key.Q;

                    //      In the unlikely case that r = 0, start again with a different random k
                } while (r.IsZero);


                //  Calculate s = ((k pow −1)(H(m) + x*r)) mod q
                k = (BigInteger.ModInverse(k, this._key.Q) * (m + this._key.X * r));

                s = k % this._key.Q;

                //  In the unlikely case that s = 0, start again with a different random k
            } while (s.IsZero);

            //  The signature is (r, s)
            return r.ToByteArray().Reverse().TrimLeadingZero().Concat(s.ToByteArray().Reverse().TrimLeadingZero()).ToArray();
        }
    }
}
