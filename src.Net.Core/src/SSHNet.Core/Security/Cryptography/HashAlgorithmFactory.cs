using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
    internal static class HashAlgorithmFactory
    {

        public static System.Security.Cryptography.MD5 CreateMD5()
        {
            return System.Security.Cryptography.MD5.Create();
        }



        public static System.Security.Cryptography.SHA1 CreateSHA1()
        {
            return System.Security.Cryptography.SHA1.Create();
        }



        public static System.Security.Cryptography.SHA256 CreateSHA256()
        {
            return System.Security.Cryptography.SHA256.Create();
        }



        public static System.Security.Cryptography.SHA384 CreateSHA384()
        {
            return  System.Security.Cryptography.SHA384.Create();
        }



        public static System.Security.Cryptography.SHA512 CreateSHA512()
        {
            return  System.Security.Cryptography.SHA512.Create();
        }



        //public static System.Security.Cryptography.RIPEMD160 CreateRIPEMD160()
        //{
        //    return  System.Security.Cryptography.RIPEMD160.Create();
        //}



        public static System.Security.Cryptography.HMACMD5 CreateHMACMD5(byte[] key)
        {
            return new System.Security.Cryptography.HMACMD5(key);
        }

        public static HMACMD5 CreateHMACMD5(byte[] key, int hashSize)
        {
            return new HMACMD5(key);
        }



        public static System.Security.Cryptography.HMACSHA1 CreateHMACSHA1(byte[] key)
        {
            return new HMACSHA1(key);
        }

        public static HMACSHA1 CreateHMACSHA1(byte[] key, int hashSize)
        {
            return new HMACSHA1(key);
        }



        public static System.Security.Cryptography.HMACSHA256 CreateHMACSHA256(byte[] key)
        {
            return new System.Security.Cryptography.HMACSHA256(key);
        }

        public static HMACSHA256 CreateHMACSHA256(byte[] key, int hashSize)
        {
            return new HMACSHA256(key);
        }



        public static System.Security.Cryptography.HMACSHA384 CreateHMACSHA384(byte[] key)
        {
            return new System.Security.Cryptography.HMACSHA384(key);
        }

        public static HMACSHA384 CreateHMACSHA384(byte[] key, int hashSize)
        {
            return new HMACSHA384(key);
        }



        public static System.Security.Cryptography.HMACSHA512 CreateHMACSHA512(byte[] key)
        {
            return new System.Security.Cryptography.HMACSHA512(key);
        }

        public static HMACSHA512 CreateHMACSHA512(byte[] key, int hashSize)
        {
            return new HMACSHA512(key);
        }



        //public static System.Security.Cryptography.HMACRIPEMD160 CreateHMACRIPEMD160(byte[] key)
        //{
        //    return new System.Security.Cryptography.HMACRIPEMD160(key);
        //}

    }
}
