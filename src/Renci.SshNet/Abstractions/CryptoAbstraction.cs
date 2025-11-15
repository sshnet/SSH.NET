using System.Security.Cryptography;

using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace Renci.SshNet.Abstractions
{
    internal static class CryptoAbstraction
    {
        internal static readonly RandomNumberGenerator Randomizer = RandomNumberGenerator.Create();

        internal static readonly SecureRandom SecureRandom = new SecureRandom(new CryptoApiRandomGenerator(Randomizer));
    }
}
