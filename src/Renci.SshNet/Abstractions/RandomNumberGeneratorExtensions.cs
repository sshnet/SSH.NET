#nullable enable
#if !NET
using Renci.SshNet.Abstractions;
#endif

namespace System.Security.Cryptography
{
    internal static class RandomNumberGeneratorExtensions
    {
        extension(RandomNumberGenerator)
        {
#if !NET
            public static byte[] GetBytes(int length)
            {
                var random = new byte[length];
                CryptoAbstraction.Randomizer.GetBytes(random);
                return random;
            }
#endif
        }
    }
}
