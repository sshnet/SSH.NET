using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements RSA cipher algorithm.
    /// </summary>
    public class RsaCipher : AsymmetricCipher
    {
        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

        private bool _isPrivate;

        private BigInteger _exponent;

        private BigInteger _modulus;
        private BigInteger _d;
        private BigInteger _dp;
        private BigInteger _dq;
        private BigInteger _inverseQ;
        private BigInteger _p;
        private BigInteger _q;

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaCipher"/> class.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <param name="modulus">The modulus.</param>
        public RsaCipher(BigInteger exponent, BigInteger modulus)
        {
            //if (key == null)
            //    throw new ArgumentNullException("key");
            //this._publicKey = key;
            this._exponent = exponent;
            this._modulus = modulus;
            this._isPrivate = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaCipher"/> class.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <param name="modulus">The modulus.</param>
        /// <param name="d">The d.</param>
        /// <param name="dp">The dp.</param>
        /// <param name="dq">The dq.</param>
        /// <param name="inverseQ">The inverse Q.</param>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        public RsaCipher(BigInteger exponent, BigInteger modulus, BigInteger d, BigInteger dp, BigInteger dq, BigInteger inverseQ, BigInteger p, BigInteger q)
        {
            //if (key == null)
            //    throw new ArgumentNullException("key");
            //this._privateKey = key;
            this._exponent = exponent;
            this._modulus = modulus;
            this._d = d;
            this._dp = dp;
            this._dq = dq;
            this._inverseQ = inverseQ;
            this._p = p;
            this._q = q;
            this._isPrivate = true;
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public override byte[] Encrypt(byte[] data)
        {
            return this.Transform(data);
        }

        /// <summary>
        /// Decrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public override byte[] Decrypt(byte[] data)
        {
            return this.Transform(data);
        }

        private byte[] Transform(byte[] data)
        {
            var bytes = new List<byte>(data.Reverse());
            bytes.Add(0);

            var input = new BigInteger(bytes.ToArray());

            BigInteger result;

            if (this._isPrivate)
            {
                BigInteger random = BigInteger.One;

                var max = this._modulus - 1;

                while (random <= BigInteger.One || random >= max)
                {
                    var bytesArray = new byte[256];
                    _randomizer.GetBytes(bytesArray);

                    bytesArray[bytesArray.Length - 1] = (byte)(bytesArray[bytesArray.Length - 1] & 0x7F);   //  Ensure not a negative value
                    random = new BigInteger(bytesArray.Reverse().ToArray());
                }

                BigInteger blindedInput = BigInteger.PositiveMod((BigInteger.ModPow(random, this._exponent, this._modulus) * input), this._modulus);

                // mP = ((input Mod p) ^ dP)) Mod p
                var mP = BigInteger.ModPow((blindedInput % this._p), this._dp, this._p);

                // mQ = ((input Mod q) ^ dQ)) Mod q
                var mQ = BigInteger.ModPow((blindedInput % this._q), this._dq, this._q);

                var h = BigInteger.PositiveMod(((mP - mQ) * this._inverseQ), this._p);

                var m = h * this._q + mQ;

                BigInteger rInv = BigInteger.ModInverse(random, this._modulus);

                result = BigInteger.PositiveMod((m * rInv), this._modulus);
            }
            else
            {
                result = BigInteger.ModPow(input, this._exponent, this._modulus);
            }
            
            return result.ToByteArray().Reverse().ToArray();
        }

    }
}
