using BenchmarkDotNet.Attributes;

using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet.Benchmarks.Security.Cryptography.Ciphers
{
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
                _privateKey = (RsaKey)((KeyHostAlgorithm) new PrivateKeyFile(s).HostKey).Key;
                
                // The implementations of RsaCipher.Encrypt/Decrypt differ based on whether the supplied RsaKey has private key information
                // or only public. So we extract out the public key information to a separate variable.
                _publicKey = new RsaKey()
                {
                    Public = _privateKey.Public
                };
            }
        }

        [Benchmark]
        public byte[] Encrypt()
        {
            return new RsaCipher(_publicKey).Encrypt(_data);
        }

        [Benchmark]
        public byte[] Decrypt()
        {
            return new RsaCipher(_privateKey).Decrypt(_data);
        }
    }
}
