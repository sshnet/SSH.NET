using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
    internal static class HashAlgorithmFactory
    {
        public static RandomNumberGenerator CreateRandomNumberGenerator()
        {
#if FEATURE_RNG_CRYPTO
            return new RNGCryptoServiceProvider();
#else
            return RandomNumberGenerator.Create();
#endif
        }

#if FEATURE_HASH_MD5
        public static System.Security.Cryptography.MD5 CreateMD5()
        {
            return System.Security.Cryptography.MD5.Create();
        }
#else
        public static Renci.Security.Cryptography.MD5 CreateMD5()
        {
            return new Renci.Security.Cryptography.MD5();
        }
#endif // FEATURE_HASH_MD5

#if FEATURE_HASH_SHA1
        public static System.Security.Cryptography.SHA1 CreateSHA1()
        {
            return new System.Security.Cryptography.SHA1Managed();
        }
#else
        public static Renci.Security.Cryptography.SHA1 CreateSHA1()
        {
            return new Renci.Security.Cryptography.SHA1();
        }
#endif

#if FEATURE_HASH_SHA256
        public static System.Security.Cryptography.SHA256 CreateSHA256()
        {
            return new System.Security.Cryptography.SHA256Managed();
        }
#else
        public static Renci.Security.Cryptography.SHA256 CreateSHA256()
        {
            return new Renci.Security.Cryptography.SHA256();
        }
#endif

#if FEATURE_HASH_SHA384
        public static System.Security.Cryptography.SHA384 CreateSHA384()
        {
            return new System.Security.Cryptography.SHA384Managed();
        }
#else
        public static Renci.Security.Cryptography.SHA384 CreateSHA384()
        {
            return new Renci.Security.Cryptography.SHA384();
        }
#endif

#if FEATURE_HASH_SHA512
        public static System.Security.Cryptography.SHA512 CreateSHA512()
        {
            return new System.Security.Cryptography.SHA512Managed();
        }
#else
        public static Renci.Security.Cryptography.SHA512 CreateSHA512()
        {
            return new Renci.Security.Cryptography.SHA512();
        }
#endif

#if FEATURE_HASH_RIPEMD160
        public static System.Security.Cryptography.RIPEMD160 CreateRIPEMD160()
        {
            return new System.Security.Cryptography.RIPEMD160Managed();
        }
#else
        public static Renci.Security.Cryptography.RIPEMD160 CreateRIPEMD160()
        {
            return new Renci.Security.Cryptography.RIPEMD160();
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
        public static Renci.Security.Cryptography.HMACMD5 CreateHMACMD5(byte[] key)
        {
            return new Renci.Security.Cryptography.HMACMD5(key);
        }

        public static Renci.Security.Cryptography.HMACMD5 CreateHMACMD5(byte[] key, int hashSize)
        {
            return new Renci.Security.Cryptography.HMACMD5(key, hashSize);
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
        public static Renci.Security.Cryptography.HMACSHA1 CreateHMACSHA1(byte[] key)
        {
            return new Renci.Security.Cryptography.HMACSHA1(key);
        }

        public static Renci.Security.Cryptography.HMACSHA1 CreateHMACSHA1(byte[] key, int hashSize)
        {
            return new Renci.Security.Cryptography.HMACSHA1(key, hashSize);
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
        public static Renci.Security.Cryptography.HMACSHA256 CreateHMACSHA256(byte[] key)
        {
            return new Renci.Security.Cryptography.HMACSHA256(key);
        }

        public static Renci.Security.Cryptography.HMACSHA256 CreateHMACSHA256(byte[] key, int hashSize)
        {
            return new Renci.Security.Cryptography.HMACSHA256(key, hashSize);
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
        public static Renci.Security.Cryptography.HMACSHA384 CreateHMACSHA384(byte[] key)
        {
            return new Renci.Security.Cryptography.HMACSHA384(key);
        }

        public static Renci.Security.Cryptography.HMACSHA384 CreateHMACSHA384(byte[] key, int hashSize)
        {
            return new Renci.Security.Cryptography.HMACSHA384(key, hashSize);
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
        public static Renci.Security.Cryptography.HMACSHA512 CreateHMACSHA512(byte[] key)
        {
            return new Renci.Security.Cryptography.HMACSHA512(key);
        }

        public static Renci.Security.Cryptography.HMACSHA512 CreateHMACSHA512(byte[] key, int hashSize)
        {
            return new Renci.Security.Cryptography.HMACSHA512(key, hashSize);
        }
#endif // FEATURE_HMAC_SHA512

#if FEATURE_HMAC_RIPEMD160
        public static System.Security.Cryptography.HMACRIPEMD160 CreateHMACRIPEMD160(byte[] key)
        {
            return new System.Security.Cryptography.HMACRIPEMD160(key);
        }
#else
        public static Renci.Security.Cryptography.HMACRIPEMD160 CreateHMACRIPEMD160(byte[] key)
        {
            return new Renci.Security.Cryptography.HMACRIPEMD160(key);
        }
#endif // FEATURE_HMAC_RIPEMD160
    }
}
