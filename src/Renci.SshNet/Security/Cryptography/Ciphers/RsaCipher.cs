using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements RSA cipher algorithm.
    /// </summary>
    public class RsaCipher : AsymmetricCipher
    {
        private readonly bool _isPrivate;

        private readonly RsaKey _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaCipher"/> class.
        /// </summary>
        /// <param name="key">The RSA key.</param>
        public RsaCipher(RsaKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            _key = key;
            _isPrivate = !_key.D.IsZero;
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="data"/>.</param>
        /// <returns>Encrypted data.</returns>
        public override byte[] Encrypt(byte[] data, int offset, int length)
        {
            //  Calculate signature
            var bitLength = _key.Modulus.BitLength;

            var paddedBlock = new byte[bitLength / 8 + (bitLength % 8 > 0 ? 1 : 0) - 1];

            paddedBlock[0] = 0x01;
            for (var i = 1; i < paddedBlock.Length - length - 1; i++)
            {
                paddedBlock[i] = 0xFF;
            }

            Buffer.BlockCopy(data, offset, paddedBlock, paddedBlock.Length - length, length);

            return Transform(paddedBlock);
        }

        /// <summary>
        /// Decrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        /// <exception cref="NotSupportedException">Only block type 01 or 02 are supported.</exception>
        /// <exception cref="NotSupportedException">Thrown when decrypted block type is not supported.</exception>
        public override byte[] Decrypt(byte[] data)
        {
            return Decrypt(data, 0, data.Length);
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="data">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin decrypting.</param>
        /// <param name="length">The number of bytes to decrypt from <paramref name="data"/>.</param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        /// <exception cref="NotSupportedException">Only block type 01 or 02 are supported.</exception>
        /// <exception cref="NotSupportedException">Thrown when decrypted block type is not supported.</exception>
        public override byte[] Decrypt(byte[] data, int offset, int length)
        {
            var paddedBlock = Transform(data, offset, length);

            if (paddedBlock[0] != 1 && paddedBlock[0] != 2)
                throw new NotSupportedException("Only block type 01 or 02 are supported.");

            var position = 1;
            while (position < paddedBlock.Length && paddedBlock[position] != 0)
                position++;
            position++;

            var result = new byte[paddedBlock.Length - position];
            Buffer.BlockCopy(paddedBlock, position, result, 0, result.Length);
            return result;
        }

        private byte[] Transform(byte[] data)
        {
            return Transform(data, 0, data.Length);
        }

        private byte[] Transform(byte[] data, int offset, int length)
        {
            Array.Reverse(data, offset, length);

            var inputBytes = new byte[length + 1];
            Buffer.BlockCopy(data, offset, inputBytes, 0, length);

            var input = new BigInteger(inputBytes);

            BigInteger result;

            if (_isPrivate)
            {
                var random = BigInteger.One;
                var max = _key.Modulus - 1;
                var bitLength = _key.Modulus.BitLength;

                if (max < BigInteger.One)
                    throw new SshException("Invalid RSA key.");

                while (random <= BigInteger.One || random >= max)
                {
                    random = BigInteger.Random(bitLength);
                }

                var blindedInput = BigInteger.PositiveMod((BigInteger.ModPow(random, _key.Exponent, _key.Modulus) * input), _key.Modulus);

                // mP = ((input Mod p) ^ dP)) Mod p
                var mP = BigInteger.ModPow((blindedInput % _key.P), _key.DP, _key.P);

                // mQ = ((input Mod q) ^ dQ)) Mod q
                var mQ = BigInteger.ModPow((blindedInput % _key.Q), _key.DQ, _key.Q);

                var h = BigInteger.PositiveMod(((mP - mQ) * _key.InverseQ), _key.P);

                var m = h * _key.Q + mQ;

                var rInv = BigInteger.ModInverse(random, _key.Modulus);

                result = BigInteger.PositiveMod((m * rInv), _key.Modulus);
            }
            else
            {
                result = BigInteger.ModPow(input, _key.Exponent, _key.Modulus);
            }

            return result.ToByteArray().Reverse();
        }
    }
}
