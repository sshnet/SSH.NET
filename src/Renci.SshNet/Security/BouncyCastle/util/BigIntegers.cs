using System;

using Renci.SshNet.Security.Org.BouncyCastle.Math;
using Renci.SshNet.Security.Org.BouncyCastle.Security;

namespace Renci.SshNet.Security.Org.BouncyCastle.Utilities
{
    /**
     * BigInteger utilities.
     */
    internal abstract class BigIntegers
    {
        private const int MaxIterations = 1000;

        /**
        * Return the passed in value as an unsigned byte array.
        *
        * @param value value to be converted.
        * @return a byte array without a leading zero byte if present in the signed encoding.
        */
        public static byte[] AsUnsignedByteArray(
            BigInteger n)
        {
            return n.ToByteArrayUnsigned();
        }

        /**
         * Return the passed in value as an unsigned byte array of specified length, zero-extended as necessary.
         *
         * @param length desired length of result array.
         * @param n value to be converted.
         * @return a byte array of specified length, with leading zeroes as necessary given the size of n.
         */
        public static byte[] AsUnsignedByteArray(int length, BigInteger n)
        {
            byte[] bytes = n.ToByteArrayUnsigned();

            if (bytes.Length > length)
                throw new ArgumentException("standard length exceeded", "n");

            if (bytes.Length == length)
                return bytes;

            byte[] tmp = new byte[length];
            Array.Copy(bytes, 0, tmp, tmp.Length - bytes.Length, bytes.Length);
            return tmp;
        }

        /**
        * Return a random BigInteger not less than 'min' and not greater than 'max'
        * 
        * @param min the least value that may be generated
        * @param max the greatest value that may be generated
        * @param random the source of randomness
        * @return a random BigInteger value in the range [min,max]
        */
        public static BigInteger CreateRandomInRange(
            BigInteger		min,
            BigInteger		max,
            // TODO Should have been just Random class
            SecureRandom	random)
        {
            int cmp = min.CompareTo(max);
            if (cmp >= 0)
            {
                if (cmp > 0)
                    throw new ArgumentException("'min' may not be greater than 'max'");

                return min;
            }

            if (min.BitLength > max.BitLength / 2)
            {
                return CreateRandomInRange(BigInteger.Zero, max.Subtract(min), random).Add(min);
            }

            for (int i = 0; i < MaxIterations; ++i)
            {
                BigInteger x = new BigInteger(max.BitLength, random);
                if (x.CompareTo(min) >= 0 && x.CompareTo(max) <= 0)
                {
                    return x;
                }
            }

            // fall back to a faster (restricted) method
            return new BigInteger(max.Subtract(min).BitLength - 1, random).Add(min);
        }

        public static int GetUnsignedByteLength(BigInteger n)
        {
            return (n.BitLength + 7) / 8;
        }
    }
}
