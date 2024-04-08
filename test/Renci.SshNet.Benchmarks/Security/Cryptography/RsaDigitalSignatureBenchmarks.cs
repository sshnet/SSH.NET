using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;

using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Benchmarks.Security.Cryptography
{
    [MemoryDiagnoser]
    public class RsaDigitalSignatureBenchmarks
    {
        private readonly RsaKey _privateKey;
        private readonly RsaKey _publicKey;
        private readonly byte[] _data;
        private readonly byte[] _signature;

        public RsaDigitalSignatureBenchmarks()
        {
            _data = new byte[128];

            Random random = new(Seed: 12345);
            random.NextBytes(_data);

            using (var s = typeof(RsaDigitalSignatureBenchmarks).Assembly.GetManifestResourceStream("Renci.SshNet.Benchmarks.Data.Key.OPENSSH.RSA.txt"))
            {
                _privateKey = (RsaKey)new PrivateKeyFile(s).Key;

                // The *former* implementations of RsaCipher.Encrypt/Decrypt differ based on whether the supplied RsaKey has private key information
                // or only public. So we extract out the public key information to a separate variable.
                _publicKey = new RsaKey(_privateKey.Modulus, _privateKey.Exponent, default, default, default, default);
            }

            _signature = new RsaDigitalSignature(_privateKey, HashAlgorithmName.SHA256).Sign(_data);
        }

        [Benchmark]
        public byte[] Sign()
        {
            return new RsaDigitalSignature(_privateKey, HashAlgorithmName.SHA256).Sign(_data);
        }

        [Benchmark]
        public bool Verify()
        {
            // The former implementation mutates (reverses) the signature... so clone it
            return new RsaDigitalSignature(_publicKey, HashAlgorithmName.SHA256).Verify(_data, (byte[])_signature.Clone());
        }
    }
}
