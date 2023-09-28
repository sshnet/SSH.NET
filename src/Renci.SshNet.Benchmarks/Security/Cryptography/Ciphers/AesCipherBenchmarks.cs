using BenchmarkDotNet.Attributes;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;

namespace Renci.SshNet.Benchmarks.Security.Cryptography.Ciphers
{
    [MemoryDiagnoser]
    public class AesCipherBenchmarks
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly byte[] _data;

        public AesCipherBenchmarks()
        {
            _key = new byte[32];
            _iv = new byte[16];
            _data = new byte[256];

            Random random = new(Seed: 12345);
            random.NextBytes(_key);
            random.NextBytes(_iv);
            random.NextBytes(_data);
        }

        [Benchmark]
        public byte[] Encrypt_CBC()
        {
            return new AesCipher(_key, new CbcCipherMode(_iv), null).Encrypt(_data);
        }

        [Benchmark]
        public byte[] Decrypt_CBC()
        {
            return new AesCipher(_key, new CbcCipherMode(_iv), null).Decrypt(_data);
        }
    }
}
