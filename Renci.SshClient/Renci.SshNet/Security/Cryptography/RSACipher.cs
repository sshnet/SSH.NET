using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// RSA algorithm implementation
    /// </summary>
    public class RSACipher : AsymmetricCipher
    {
        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

        private RSAPublicKey _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="RSACipher"/> class.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <param name="key">The key.</param>
        public RSACipher(HashAlgorithm hash, RSAPublicKey key)
        {
            this._key = key;
        }

        /// <summary>
        /// Transforms the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public override byte[] Transform(byte[] data)
        {
            var bytes = new List<byte>(data.Reverse());
            bytes.Add(0);
            return this.Transform(new BigInteger(bytes.ToArray())).ToByteArray().Reverse().ToArray();
        }

        /// <summary>
        /// Transforms the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public override BigInteger Transform(BigInteger input)
        {
            var privateKey = this._key as RSAPrivateKey;

            if (privateKey != null)
            {
                BigInteger random = BigInteger.One;

                var max = this._key.Modulus - 1;

                while (random <= BigInteger.One || random >= max)
                {
                    var bytesArray = new byte[256];
                    _randomizer.GetBytes(bytesArray);

                    bytesArray[bytesArray.Length - 1] = (byte)(bytesArray[bytesArray.Length - 1] & 0x7F);   //  Ensure not a negative value
                    random = new BigInteger(bytesArray.Reverse().ToArray());
                }

                BigInteger blindedInput = BigInteger.PositiveMod((BigInteger.ModPow(random, this._key.Exponent, this._key.Modulus) * input), this._key.Modulus);

                // mP = ((input Mod p) ^ dP)) Mod p
                var mP = BigInteger.ModPow((blindedInput % privateKey.P), privateKey.DP, privateKey.P);

                // mQ = ((input Mod q) ^ dQ)) Mod q
                var mQ = BigInteger.ModPow((blindedInput % privateKey.Q), privateKey.DQ, privateKey.Q);

                var h = BigInteger.PositiveMod(((mP - mQ) * privateKey.InverseQ), privateKey.P);

                var m = h * privateKey.Q + mQ;

                BigInteger rInv = BigInteger.ModInverse(random, this._key.Modulus);

                return BigInteger.PositiveMod((m * rInv), this._key.Modulus);
            }
            else
            {
                var value = BigInteger.ModPow(input, this._key.Exponent, this._key.Modulus);
                return value;
            }
        }
    }
}
