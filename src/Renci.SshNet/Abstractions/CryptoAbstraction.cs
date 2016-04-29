using System;

namespace Renci.SshNet.Security.Cryptography
{
    internal static class HashAlgorithmFactory
    {
#if FEATURE_RNG_CREATE || FEATURE_RNG_CSP
        private static readonly System.Security.Cryptography.RandomNumberGenerator Randomizer = HashAlgorithmFactory.CreateRandomNumberGenerator();
#endif

        /// <summary>
        /// Fills an array of bytes with a cryptographically strong random sequence of values.
        /// </summary>
        /// <param name="data">The array to fill with cryptographically strong random bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        /// <remarks>
        /// The length of the byte array determines how many random bytes are produced.
        /// </remarks>
        public static void GenerateRandom(byte[] data)
        {
#if FEATURE_RNG_CREATE || FEATURE_RNG_CSP
            Randomizer.GetBytes(data);
#else
            if(data == null)
                throw new ArgumentNullException("data");

            var buffer = Windows.Security.Cryptography.CryptographicBuffer.GenerateRandom((uint) data.Length);
            System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.CopyTo(buffer, data);
#endif
        }

#if FEATURE_RNG_CREATE || FEATURE_RNG_CSP
        public static System.Security.Cryptography.RandomNumberGenerator CreateRandomNumberGenerator()
        {
#if FEATURE_RNG_CREATE
            return System.Security.Cryptography.RandomNumberGenerator.Create();
#elif FEATURE_RNG_CSP
            return new System.Security.Cryptography.RNGCryptoServiceProvider();
#else
#error Creation of RandomNumberGenerator is not implemented.
#endif
        }
#endif // FEATURE_RNG_CREATE || FEATURE_RNG_CSP

#if FEATURE_HASH_MD5
        public static System.Security.Cryptography.MD5 CreateMD5()
        {
            return System.Security.Cryptography.MD5.Create();
        }
#else
        public static global::SshNet.Security.Cryptography.MD5 CreateMD5()
        {
            return new global::SshNet.Security.Cryptography.MD5();
        }
#endif // FEATURE_HASH_MD5

#if FEATURE_HASH_SHA1
        public static System.Security.Cryptography.SHA1 CreateSHA1()
        {
            return new System.Security.Cryptography.SHA1Managed();
        }
#else
        public static global::SshNet.Security.Cryptography.SHA1 CreateSHA1()
        {
            return new global::SshNet.Security.Cryptography.SHA1();
        }
#endif

#if FEATURE_HASH_SHA256
        public static System.Security.Cryptography.SHA256 CreateSHA256()
        {
            return new System.Security.Cryptography.SHA256Managed();
        }
#else
        public static global::SshNet.Security.Cryptography.SHA256 CreateSHA256()
        {
            return new global::SshNet.Security.Cryptography.SHA256();
        }
#endif

#if FEATURE_HASH_SHA384
        public static System.Security.Cryptography.SHA384 CreateSHA384()
        {
            return new System.Security.Cryptography.SHA384Managed();
        }
#else
        public static global::SshNet.Security.Cryptography.SHA384 CreateSHA384()
        {
            return new global::SshNet.Security.Cryptography.SHA384();
        }
#endif

#if FEATURE_HASH_SHA512
        public static System.Security.Cryptography.SHA512 CreateSHA512()
        {
            return new System.Security.Cryptography.SHA512Managed();
        }
#else
        public static global::SshNet.Security.Cryptography.SHA512 CreateSHA512()
        {
            return new global::SshNet.Security.Cryptography.SHA512();
        }
#endif

#if FEATURE_HASH_RIPEMD160
        public static System.Security.Cryptography.RIPEMD160 CreateRIPEMD160()
        {
            return new System.Security.Cryptography.RIPEMD160Managed();
        }
#else
        public static global::SshNet.Security.Cryptography.RIPEMD160 CreateRIPEMD160()
        {
            return new global::SshNet.Security.Cryptography.RIPEMD160();
        }
#endif // FEATURE_HASH_RIPEMD160

#if FEATURE_HMAC_MD5
        public static System.Security.Cryptography.HMACMD5 CreateHMACMD5(byte[] key)
        {
            return new System.Security.Cryptography.HMACMD5(key);
        }

        public static HMACMD5 CreateHMACMD5(byte[] key, int hashSize)
        {
            return new HMACMD5(key, hashSize);
        }
#else
        public static global::SshNet.Security.Cryptography.HMACMD5 CreateHMACMD5(byte[] key)
        {
            return new global::SshNet.Security.Cryptography.HMACMD5(key);
        }

        public static global::SshNet.Security.Cryptography.HMACMD5 CreateHMACMD5(byte[] key, int hashSize)
        {
            return new global::SshNet.Security.Cryptography.HMACMD5(key, hashSize);
        }
#endif // FEATURE_HMAC_MD5

#if FEATURE_HMAC_SHA1
        public static System.Security.Cryptography.HMACSHA1 CreateHMACSHA1(byte[] key)
        {
            return new System.Security.Cryptography.HMACSHA1(key);
        }

        public static HMACSHA1 CreateHMACSHA1(byte[] key, int hashSize)
        {
            return new HMACSHA1(key, hashSize);
        }
#else
        public static global::SshNet.Security.Cryptography.HMACSHA1 CreateHMACSHA1(byte[] key)
        {
            return new global::SshNet.Security.Cryptography.HMACSHA1(key);
        }

        public static global::SshNet.Security.Cryptography.HMACSHA1 CreateHMACSHA1(byte[] key, int hashSize)
        {
            return new global::SshNet.Security.Cryptography.HMACSHA1(key, hashSize);
        }
#endif // FEATURE_HMAC_SHA1

#if FEATURE_HMAC_SHA256
        public static System.Security.Cryptography.HMACSHA256 CreateHMACSHA256(byte[] key)
        {
            return new System.Security.Cryptography.HMACSHA256(key);
        }

        public static HMACSHA256 CreateHMACSHA256(byte[] key, int hashSize)
        {
            return new HMACSHA256(key, hashSize);
        }
#else
        public static global::SshNet.Security.Cryptography.HMACSHA256 CreateHMACSHA256(byte[] key)
        {
            return new global::SshNet.Security.Cryptography.HMACSHA256(key);
        }

        public static global::SshNet.Security.Cryptography.HMACSHA256 CreateHMACSHA256(byte[] key, int hashSize)
        {
            return new global::SshNet.Security.Cryptography.HMACSHA256(key, hashSize);
        }
#endif // FEATURE_HMAC_SHA256

#if FEATURE_HMAC_SHA384
        public static System.Security.Cryptography.HMACSHA384 CreateHMACSHA384(byte[] key)
        {
            return new System.Security.Cryptography.HMACSHA384(key);
        }

        public static HMACSHA384 CreateHMACSHA384(byte[] key, int hashSize)
        {
            return new HMACSHA384(key, hashSize);
        }
#else
        public static global::SshNet.Security.Cryptography.HMACSHA384 CreateHMACSHA384(byte[] key)
        {
            return new global::SshNet.Security.Cryptography.HMACSHA384(key);
        }

        public static global::SshNet.Security.Cryptography.HMACSHA384 CreateHMACSHA384(byte[] key, int hashSize)
        {
            return new global::SshNet.Security.Cryptography.HMACSHA384(key, hashSize);
        }
#endif // FEATURE_HMAC_SHA384

#if FEATURE_HMAC_SHA512
        public static System.Security.Cryptography.HMACSHA512 CreateHMACSHA512(byte[] key)
        {
            return new System.Security.Cryptography.HMACSHA512(key);
        }

        public static HMACSHA512 CreateHMACSHA512(byte[] key, int hashSize)
        {
            return new HMACSHA512(key, hashSize);
        }
#else
        public static global::SshNet.Security.Cryptography.HMACSHA512 CreateHMACSHA512(byte[] key)
        {
            return new global::SshNet.Security.Cryptography.HMACSHA512(key);
        }

        public static global::SshNet.Security.Cryptography.HMACSHA512 CreateHMACSHA512(byte[] key, int hashSize)
        {
            return new global::SshNet.Security.Cryptography.HMACSHA512(key, hashSize);
        }
#endif // FEATURE_HMAC_SHA512

#if FEATURE_HMAC_RIPEMD160
        public static System.Security.Cryptography.HMACRIPEMD160 CreateHMACRIPEMD160(byte[] key)
        {
            return new System.Security.Cryptography.HMACRIPEMD160(key);
        }
#else
        public static global::SshNet.Security.Cryptography.HMACRIPEMD160 CreateHMACRIPEMD160(byte[] key)
        {
            return new global::SshNet.Security.Cryptography.HMACRIPEMD160(key);
        }
#endif // FEATURE_HMAC_RIPEMD160
    }
}
