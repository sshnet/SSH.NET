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
    }
}
