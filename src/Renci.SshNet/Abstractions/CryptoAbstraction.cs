using System.Security.Cryptography;

namespace Renci.SshNet.Abstractions
{
    internal static class CryptoAbstraction
    {
        private static readonly RandomNumberGenerator Randomizer = RandomNumberGenerator.Create();

        /// <summary>
        /// Generates a <see cref="byte"/> array of the specified length, and fills it with a
        /// cryptographically strong random sequence of values.
        /// </summary>
        /// <param name="length">The length of the array generate.</param>
        public static byte[] GenerateRandom(int length)
        {
            var random = new byte[length];
            Randomizer.GetBytes(random);
            return random;
        }

        public static byte[] HashMD5(byte[] source)
        {
#if NET
            return MD5.HashData(source);
#else
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(source);
            }
#endif
        }

        public static byte[] HashSHA1(byte[] source)
        {
#if NET
            return SHA1.HashData(source);
#else
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(source);
            }
#endif
        }

        public static byte[] HashSHA256(byte[] source)
        {
#if NET
            return SHA256.HashData(source);
#else
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(source);
            }
#endif
        }

        public static byte[] HashSHA384(byte[] source)
        {
#if NET
            return SHA384.HashData(source);
#else
            using (var sha384 = SHA384.Create())
            {
                return sha384.ComputeHash(source);
            }
#endif
        }

        public static byte[] HashSHA512(byte[] source)
        {
#if NET
            return SHA512.HashData(source);
#else
            using (var sha512 = SHA512.Create())
            {
                return sha512.ComputeHash(source);
            }
#endif
        }
    }
}
