using System;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Abstractions
{
    internal static class CryptoAbstraction
    {
        private static readonly System.Security.Cryptography.RandomNumberGenerator Randomizer = CreateRandomNumberGenerator();

        /// <summary>
        /// Generates a <see cref="byte"/> array of the specified length, and fills it with a
        /// cryptographically strong random sequence of values.
        /// </summary>
        /// <param name="length">The length of the array generate.</param>
        public static byte[] GenerateRandom(int length)
        {
            var random = new byte[length];
            GenerateRandom(random);
            return random;
        }

        /// <summary>
        /// Fills an array of bytes with a cryptographically strong random sequence of values.
        /// </summary>
        /// <param name="data">The array to fill with cryptographically strong random bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The length of the byte array determines how many random bytes are produced.
        /// </remarks>
        public static void GenerateRandom(byte[] data)
        {
            Randomizer.GetBytes(data);
        }

        public static System.Security.Cryptography.RandomNumberGenerator CreateRandomNumberGenerator()
        {
            return System.Security.Cryptography.RandomNumberGenerator.Create();
        }

        public static System.Security.Cryptography.MD5 CreateMD5()
        {
#pragma warning disable CA5351 // Do not use broken cryptographic algorithms
            return System.Security.Cryptography.MD5.Create();
#pragma warning restore CA5351 // Do not use broken cryptographic algorithms
        }

        public static System.Security.Cryptography.SHA1 CreateSHA1()
        {
#pragma warning disable CA5350 // Do not use weak cryptographic algorithms
            return System.Security.Cryptography.SHA1.Create();
#pragma warning restore CA5350 // Do not use weak cryptographic algorithms
        }

        public static System.Security.Cryptography.SHA256 CreateSHA256()
        {
            return System.Security.Cryptography.SHA256.Create();
        }

        public static System.Security.Cryptography.SHA384 CreateSHA384()
        {
            return System.Security.Cryptography.SHA384.Create();
        }

        public static System.Security.Cryptography.SHA512 CreateSHA512()
        {
            return System.Security.Cryptography.SHA512.Create();
        }

#if FEATURE_HASH_RIPEMD160_CREATE || FEATURE_HASH_RIPEMD160_MANAGED
        public static System.Security.Cryptography.RIPEMD160 CreateRIPEMD160()
        {
#if FEATURE_HASH_RIPEMD160_CREATE
#pragma warning disable CA5350 // Do not use weak cryptographic algorithms
            return System.Security.Cryptography.RIPEMD160.Create();
#pragma warning restore CA5350 // Do not use weak cryptographic algorithms
#else
            return new System.Security.Cryptography.RIPEMD160Managed();
#endif
        }
#else
        public static global::SshNet.Security.Cryptography.RIPEMD160 CreateRIPEMD160()
        {
            return new global::SshNet.Security.Cryptography.RIPEMD160();
        }
#endif // FEATURE_HASH_RIPEMD160

        public static HMAC CreateHMACMD5(byte[] key, bool etm)
        {
#pragma warning disable CA5351 // Do not use broken cryptographic algorithms
            return new HMAC(new System.Security.Cryptography.HMACMD5(key), etm);
#pragma warning restore CA5351 // Do not use broken cryptographic algorithms
        }

        public static HMAC CreateHMACMD5(byte[] key, int hashSize, bool etm)
        {
#pragma warning disable CA5351 // Do not use broken cryptographic algorithms
            return new HMAC(new HMACMD5(key, hashSize), etm);
#pragma warning restore CA5351 // Do not use broken cryptographic algorithms
        }

        public static HMAC CreateHMACSHA1(byte[] key, bool etm)
        {
#pragma warning disable CA5350 // Do not use weak cryptographic algorithms
            return new HMAC(new System.Security.Cryptography.HMACSHA1(key), etm);
#pragma warning restore CA5350 // Do not use weak cryptographic algorithms
        }

        public static HMAC CreateHMACSHA1(byte[] key, int hashSize, bool etm)
        {
#pragma warning disable CA5350 // Do not use weak cryptographic algorithms
            return new HMAC(new HMACSHA1(key, hashSize), etm);
#pragma warning restore CA5350 // Do not use weak cryptographic algorithms
        }

        public static HMAC CreateHMACSHA256(byte[] key, bool etm)
        {
            return new HMAC(new System.Security.Cryptography.HMACSHA256(key), etm);
        }

        public static HMAC CreateHMACSHA256(byte[] key, int hashSize, bool etm)
        {
            return new HMAC(new HMACSHA256(key, hashSize), etm);
        }

        public static HMAC CreateHMACSHA384(byte[] key, bool etm)
        {
            return new HMAC(new System.Security.Cryptography.HMACSHA384(key), etm);
        }

        public static HMAC CreateHMACSHA384(byte[] key, int hashSize, bool etm)
        {
            return new HMAC(new HMACSHA384(key, hashSize), etm);
        }

        public static HMAC CreateHMACSHA512(byte[] key, bool etm)
        {
            return new HMAC(new System.Security.Cryptography.HMACSHA512(key), etm);
        }

        public static HMAC CreateHMACSHA512(byte[] key, int hashSize, bool etm)
        {
            return new HMAC(new HMACSHA512(key, hashSize), etm);
        }

#if FEATURE_HMAC_RIPEMD160
        public static HMAC CreateHMACRIPEMD160(byte[] key, bool etm)
        {
#pragma warning disable CA5350 // Do not use weak cryptographic algorithms
            return new HMAC(new System.Security.Cryptography.HMACRIPEMD160(key), etm);
#pragma warning restore CA5350 // Do not use weak cryptographic algorithms
        }
#else
        public static HMAC CreateHMACRIPEMD160(byte[] key, bool etm)
        {
            return new HMAC(new global::SshNet.Security.Cryptography.HMACRIPEMD160(key), etm);
        }
#endif // FEATURE_HMAC_RIPEMD160
    }
}
