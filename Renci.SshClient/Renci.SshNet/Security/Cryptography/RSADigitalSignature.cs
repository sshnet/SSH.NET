using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    public class RSADigitalSignature : DigitalSignature
    {
        private HashAlgorithm _hash;
        private RSACipher _cipher;

        public RSADigitalSignature(RSAPublicKey key)
        {
            this._hash = new SHA1Hash();
            this._cipher = new RSACipher(this._hash, key);
        }

        public override bool VerifySignature(byte[] input, byte[] signature)
        {
            var sig = this._cipher.Transform(signature);

            //  TODO:   Ensure that only 1 or 2 types are supported
            var position = 1;
            while (position < sig.Length && sig[position] != 0)
                position++;
            position++;


            var sig1 = new byte[sig.Length - position];

            Array.Copy(sig, position, sig1, 0, sig1.Length);

            var hashData = this._hash.ComputeHash(input);

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

        public override byte[] CreateSignature(byte[] input)
        {
            //  Calculate hash value
            var hashData = this._hash.ComputeHash(input);

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

            var input1 = new BigInteger(rsaInputBlockSize.Reverse().ToArray());

            return this._cipher.Transform(input1).ToByteArray().Reverse().TrimLeadingZero().ToArray();
        }

        private static List<byte> DerEncode(byte[] hashData)
        {
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
