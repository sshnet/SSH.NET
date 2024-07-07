﻿using System;

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
            return System.Security.Cryptography.SHA384.Create();
        }

        public static System.Security.Cryptography.SHA512 CreateSHA512()
        {
            return System.Security.Cryptography.SHA512.Create();
        }
    }
}
