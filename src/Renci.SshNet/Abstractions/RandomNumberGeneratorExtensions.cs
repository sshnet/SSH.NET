#nullable enable
namespace System.Security.Cryptography
{
    internal static class RandomNumberGeneratorExtensions
    {
#if !NET
        private static readonly RandomNumberGenerator Randomizer = RandomNumberGenerator.Create();
#endif

        extension(RandomNumberGenerator)
        {
#if !NET
            public static byte[] GetBytes(int length)
            {
                var random = new byte[length];
                Randomizer.GetBytes(random);
                return random;
            }
#endif
        }
    }
}
