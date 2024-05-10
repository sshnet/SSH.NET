using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;

using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Benchmarks.Security.Cryptography
{
    [MemoryDiagnoser]
    public class RsaDigitalSignatureBenchmarks
    {
        private readonly RsaKey _key;
        private readonly byte[] _data;
        private readonly byte[] _signature;

        public RsaDigitalSignatureBenchmarks()
        {
            _data = new byte[128];

            Random random = new(Seed: 12345);
            random.NextBytes(_data);

            using (var s = typeof(RsaDigitalSignatureBenchmarks).Assembly.GetManifestResourceStream("Renci.SshNet.Benchmarks.Data.Key.OPENSSH.RSA.txt"))
            {
                _key = (RsaKey) new PrivateKeyFile(s).Key;
            }
            _signature = new RsaDigitalSignature(_key, HashAlgorithmName.SHA256).Sign(_data);
        }

        [Benchmark]
        public byte[] Sign()
        {
            return new RsaDigitalSignature(_key, HashAlgorithmName.SHA256).Sign(_data);
        }

        [Benchmark]
        public bool Verify()
        {
            return new RsaDigitalSignature(_key, HashAlgorithmName.SHA256).Verify(_data, _signature);
        }
    }
}
