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
    }
}
