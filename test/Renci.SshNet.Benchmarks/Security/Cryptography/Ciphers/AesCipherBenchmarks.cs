using BenchmarkDotNet.Attributes;
using Renci.SshNet.Security.Cryptography.Ciphers;

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
            _data = new byte[32 * 1024];

            Random random = new(Seed: 12345);
            random.NextBytes(_key);
            random.NextBytes(_iv);
            random.NextBytes(_data);
        }

        [Benchmark]
        public byte[] Encrypt_CBC()
        {
            return new AesCipher(_key, _iv, AesCipherMode.CBC, false).Encrypt(_data);
        }

        [Benchmark]
        public byte[] Decrypt_CBC()
        {
            return new AesCipher(_key, _iv, AesCipherMode.CBC, false).Decrypt(_data);
        }

        [Benchmark]
        public byte[] Encrypt_CFB()
        {
            return new AesCipher(_key, _iv, AesCipherMode.CFB, false).Encrypt(_data);
        }

        [Benchmark]
        public byte[] Decrypt_CFB()
        {
            return new AesCipher(_key, _iv, AesCipherMode.CFB, false).Decrypt(_data);
        }

        [Benchmark]
        public byte[] Encrypt_CTR()
        {
            return new AesCipher(_key, _iv, AesCipherMode.CTR, false).Encrypt(_data);
        }

        [Benchmark]
        public byte[] Decrypt_CTR()
        {
            return new AesCipher(_key, _iv, AesCipherMode.CTR, false).Decrypt(_data);
        }

        [Benchmark]
        public byte[] Encrypt_ECB()
        {
            return new AesCipher(_key, null, AesCipherMode.ECB, false).Encrypt(_data);
        }

        [Benchmark]
        public byte[] Decrypt_ECB()
        {
            return new AesCipher(_key, null, AesCipherMode.ECB, false).Decrypt(_data);
        }
    }
}
