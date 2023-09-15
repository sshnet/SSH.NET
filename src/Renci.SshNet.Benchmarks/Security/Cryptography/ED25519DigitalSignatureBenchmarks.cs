using BenchmarkDotNet.Attributes;

using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Benchmarks.Security.Cryptography
{
    [MemoryDiagnoser]
    public class ED25519DigitalSignatureBenchmarks
    {
        private readonly ED25519Key _key;
        private readonly byte[] _data;
        private readonly byte[] _signature;

        public ED25519DigitalSignatureBenchmarks()
        {
            _data = new byte[128];

            Random random = new(Seed: 12345);
            random.NextBytes(_data);

            using (var s = typeof(ED25519DigitalSignatureBenchmarks).Assembly.GetManifestResourceStream("Renci.SshNet.Benchmarks.Data.Key.OPENSSH.ED25519.txt"))
            {
                _key = (ED25519Key) ((KeyHostAlgorithm) new PrivateKeyFile(s).HostKey).Key;
            }
            _signature = new ED25519DigitalSignature(_key).Sign(_data);
        }

        [Benchmark]
        public byte[] Sign()
        {
            return new ED25519DigitalSignature(_key).Sign(_data);
        }

        [Benchmark]
        public bool Verify()
        {
            return new ED25519DigitalSignature(_key).Verify(_data, _signature);
        }
    }
}
