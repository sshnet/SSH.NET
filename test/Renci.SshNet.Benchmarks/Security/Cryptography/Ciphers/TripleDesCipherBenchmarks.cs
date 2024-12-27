using BenchmarkDotNet.Attributes;

using Renci.SshNet.Security.Cryptography.Ciphers;

using CipherMode = System.Security.Cryptography.CipherMode;

namespace Renci.SshNet.Benchmarks.Security.Cryptography.Ciphers
{
    [MemoryDiagnoser]
    public class TripleDesCipherBenchmarks
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly byte[] _data;

        public TripleDesCipherBenchmarks()
        {
            _key = new byte[24];
            _iv = new byte[8];
            _data = new byte[32 * 1024];

            Random random = new(Seed: 12345);
            random.NextBytes(_key);
            random.NextBytes(_iv);
            random.NextBytes(_data);
        }

        [Benchmark]
        public byte[] Encrypt_CBC()
        {
            return new TripleDesCipher(_key, _iv, CipherMode.CBC, false).Encrypt(_data);
        }

        [Benchmark]
        public byte[] Decrypt_CBC()
        {
            return new TripleDesCipher(_key, _iv, CipherMode.CBC, false).Decrypt(_data);
        }

        [Benchmark]
        public byte[] Encrypt_CFB()
        {
            return new TripleDesCipher(_key, _iv, CipherMode.CFB, false).Encrypt(_data);
        }

        [Benchmark]
        public byte[] Decrypt_CFB()
        {
            return new TripleDesCipher(_key, _iv, CipherMode.CFB, false).Decrypt(_data);
        }
    }
}
