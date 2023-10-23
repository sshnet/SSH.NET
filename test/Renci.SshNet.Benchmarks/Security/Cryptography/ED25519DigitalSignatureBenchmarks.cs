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
#pragma warning disable CA2000 // Dispose objects before losing scope
                _key = (ED25519Key) new PrivateKeyFile(s).Key;
#pragma warning restore CA2000 // Dispose objects before losing scope
            }

            using (var digitalSignature = new ED25519DigitalSignature(_key))
            {
                _signature = digitalSignature.Sign(_data);
            }
        }

        [Benchmark]
        public byte[] Sign()
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            return new ED25519DigitalSignature(_key).Sign(_data);
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        [Benchmark]
        public bool Verify()
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            return new ED25519DigitalSignature(_key).Verify(_data, _signature);
#pragma warning restore CA2000 // Dispose objects before losing scope
        }
    }
}
