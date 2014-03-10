using System;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements DSA digital signature algorithm.
    /// </summary>
    public class DsaDigitalSignature : DigitalSignature, IDisposable
    {
        private HashAlgorithm _hash;

        private readonly DsaKey _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaDigitalSignature" /> class.
        /// </summary>
        /// <param name="key">The DSA key.</param>
        /// <exception cref="System.ArgumentNullException">key</exception>
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
        /// <returns>
        ///   <c>True</c> if signature was successfully verified; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Invalid signature.</exception>
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
        /// <returns>
        /// Signed input data.
        /// </returns>
        /// <exception cref="SshException">Invalid DSA key.</exception>
        public override byte[] Sign(byte[] input)
        {
            var hashInput = this._hash.ComputeHash(input);

            BigInteger m = new BigInteger(hashInput.Reverse().Concat(new byte[] { 0 }).ToArray());

            BigInteger s;
            BigInteger r;

            do
            {
                BigInteger k = BigInteger.Zero;

                do
                {
                    //  Generate a random per-message value k where 0 < k < q
                    var bitLength = this._key.Q.BitLength;

                    if (this._key.Q < BigInteger.Zero)
                        throw new SshException("Invalid DSA key.");

                    while (k <= 0 || k >= this._key.Q)
                    {
                        k = BigInteger.Random(bitLength);
                    }

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
            var signature = new byte[40];

            // issue #1918: pad part with zero's on the left if length is less than 20
            var rBytes = r.ToByteArray().Reverse().TrimLeadingZero().ToArray();
            Array.Copy(rBytes, 0, signature, 20 - rBytes.Length, rBytes.Length);

            // issue #1918: pad part with zero's on the left if length is less than 20
            var sBytes = s.ToByteArray().Reverse().TrimLeadingZero().ToArray();
            Array.Copy(sBytes, 0, signature, 40 - sBytes.Length, sBytes.Length);

            return signature;
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged ResourceMessages.
                if (disposing)
                {
                    // Dispose managed ResourceMessages.
                    if (this._hash != null)
                    {
                        this._hash.Clear();
                        this._hash = null;
                    }
                }

                // Note disposing has been done.
                this._isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SshCommand"/> is reclaimed by garbage collection.
        /// </summary>
        ~DsaDigitalSignature()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
