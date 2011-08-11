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
        private BigInteger _p;
        private BigInteger _q;
        private BigInteger _g;
        private BigInteger _privateKey;
        private BigInteger _publicKey;
        private HashAlgorithm _hash;

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaDigitalSignature"/> class.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        /// <param name="g">The g.</param>
        /// <param name="privateKey">The private key.</param>
        /// <param name="publicKey">The public key.</param>
        public DsaDigitalSignature(byte[] p, byte[] q, byte[] g, byte[] privateKey, byte[] publicKey)
        {
            this._p = new BigInteger(p.Reverse().ToArray());
            this._q = new BigInteger(q.Reverse().ToArray());
            this._g = new BigInteger(g.Reverse().ToArray());
            if (privateKey != null)
                this._privateKey = new BigInteger(privateKey.Reverse().ToArray());
            if (publicKey != null)
                this._publicKey = new BigInteger(publicKey.Reverse().ToArray());
            this._hash = new SHA1Hash();
        }

        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="signature">The signature.</param>
        /// <returns></returns>
        public override bool VerifySignature(byte[] input, byte[] signature)
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
            if (r <= 0 || r >= this._q)
                return false;

            if (s <= 0 || s >= this._q)
                return false;

            //  Calculate w = s−1 mod q
            BigInteger w = BigInteger.ModInverse(s, this._q);

            //  Calculate u1 = H(m)·w mod q
            BigInteger u1 = hm * w % this._q;

            //  Calculate u2 = r * w mod q
            BigInteger u2 = r * w % this._q;

            u1 = BigInteger.ModPow(this._g, u1, this._p);
            u2 = BigInteger.ModPow(this._publicKey, u2, this._p);

            //  Calculate v = ((g pow u1 * y pow u2) mod p) mod q
            BigInteger v = ((u1 * u2) % this._p) % this._q;

            //  The signature is valid if v = r
            return v == r;
        }

        /// <summary>
        /// Creates the signature.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public override byte[] CreateSignature(byte[] input)
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
                    //  TODO:   Take random function to base class
                    //  Generate a random per-message value k where 0 < k < q
                    do
                    {
                        //k = new BigInteger(q.BitLength, random);
                        k = BigInteger.Parse("980263959677973875983479554308083464979482795347", System.Globalization.NumberStyles.None, CultureInfo.InvariantCulture);
                    }
                    while (k <= 0 || k >= this._q);

                    //  Calculate r = ((g pow k) mod p) mod q
                    r = BigInteger.ModPow(this._g, k, this._p) % this._q;

                    //      In the unlikely case that r = 0, start again with a different random k
                } while (r.IsZero);


                //  Calculate s = ((k pow −1)(H(m) + x*r)) mod q
                k = (BigInteger.ModInverse(k, this._q) * (m + this._privateKey * r));

                s = k % this._q;

                //  In the unlikely case that s = 0, start again with a different random k
            } while (s.IsZero);

            //  The signature is (r, s)
            return r.ToByteArray().Reverse().TrimLeadingZero().Concat(s.ToByteArray().Reverse().TrimLeadingZero()).ToArray();
        }
    }
}
