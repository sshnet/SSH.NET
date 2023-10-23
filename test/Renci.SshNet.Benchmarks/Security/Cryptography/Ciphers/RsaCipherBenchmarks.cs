using BenchmarkDotNet.Attributes;

using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet.Benchmarks.Security.Cryptography.Ciphers
{
    [MemoryDiagnoser]
    public class RsaCipherBenchmarks
    {
        private readonly RsaKey _privateKey;
        private readonly RsaKey _publicKey;
        private readonly byte[] _data;

        public RsaCipherBenchmarks()
        {
            _data = new byte[128];

            Random random = new(Seed: 12345);
            random.NextBytes(_data);

            using (var s = typeof(RsaCipherBenchmarks).Assembly.GetManifestResourceStream("Renci.SshNet.Benchmarks.Data.Key.RSA.txt"))
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                _privateKey = (RsaKey) new PrivateKeyFile(s).Key;
#pragma warning restore CA2000 // Dispose objects before losing scope
            }

            // The implementations of RsaCipher.Encrypt/Decrypt differ based on whether the supplied RsaKey has private key information
            // or only public. So we extract out the public key information to a separate variable.
            _publicKey = new RsaKey()
                {
                    Public = _privateKey.Public
                };
        }

        [Benchmark]
        public byte[] Encrypt()
        {
            return new RsaCipher(_publicKey).Encrypt(_data);
        }

        // RSA Decrypt does not work
        // [Benchmark]
        public byte[] Decrypt()
        {
             return new RsaCipher(_privateKey).Decrypt(_data);
        }
    }
}
