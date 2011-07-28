using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    public class RSACipher : AsymmetricCipher
    {
        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

        private RSAPublicKey _key;

        public RSACipher(HashAlgorithm hash, RSAPublicKey key)
        {
            this._key = key;
        }

        public override byte[] Transform(byte[] data)
        {
            var bytes = new List<byte>(data.Reverse());
            bytes.Add(0);
            return this.Transform(new BigInteger(bytes.ToArray())).ToByteArray().Reverse().ToArray();
        }

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
