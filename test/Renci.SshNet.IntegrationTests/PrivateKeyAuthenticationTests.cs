using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class PrivateKeyAuthenticationTests : TestBase
    {
        private IConnectionInfoFactory _connectionInfoFactory;
        private RemoteSshdConfig _remoteSshdConfig;

        [TestInitialize]
        public void SetUp()
        {
            _connectionInfoFactory = new LinuxVMConnectionFactory(SshServerHostName, SshServerPort);
            _remoteSshdConfig = new RemoteSshd(new LinuxAdminConnectionFactory(SshServerHostName, SshServerPort)).OpenConfig();
        }

        [TestCleanup]
        public void TearDown()
        {
            _remoteSshdConfig?.Reset();
        }

        [TestMethod]
        public void SshDss()
        {
            DoTest(PublicKeyAlgorithm.SshDss, "Data.Key.SSH2.DSA.Encrypted.Des.CBC.12345.txt", "12345");
        }

        [TestMethod]
        public void SshRsa()
        {
            DoTest(PublicKeyAlgorithm.SshRsa, "Data.Key.RSA.txt");
        }

        [TestMethod]
        public void SshRsaSha256()
        {
            DoTest(PublicKeyAlgorithm.RsaSha2256, "Data.Key.RSA.txt");
        }

        [TestMethod]
        public void SshRsaSha512()
        {
            DoTest(PublicKeyAlgorithm.RsaSha2512, "Data.Key.RSA.txt");
        }

        [TestMethod]
        public void Ecdsa256()
        {
            DoTest(PublicKeyAlgorithm.EcdsaSha2Nistp256, "Data.Key.ECDSA.Encrypted.txt", "12345");
        }

        [TestMethod]
        public void Ecdsa384()
        {
            DoTest(PublicKeyAlgorithm.EcdsaSha2Nistp384, "Data.Key.OPENSSH.ECDSA384.Encrypted.txt", "12345");
        }

        [TestMethod]
        public void Ecdsa521()
        {
            DoTest(PublicKeyAlgorithm.EcdsaSha2Nistp521, "Data.Key.OPENSSH.ECDSA521.Encrypted.txt", "12345");
        }

        [TestMethod]
        public void Ed25519()
        {
            DoTest(PublicKeyAlgorithm.SshEd25519, "Data.Key.OPENSSH.ED25519.Encrypted.txt", "12345");
        }

        // The private keys used for the certificate tests below should stay out of authorized_keys for a proper test.

        [TestMethod]
        public void SshRsaCertificate()
        {
            // ssh-keygen -L -f Key.OPENSSH.RSA.Encrypted.Aes.192.CTR-cert.pub
            //    Type: ssh-rsa-cert-v01@openssh.com user certificate
            //    Public key: RSA-CERT SHA256:MMIzDVhQHqU9SAZ8p3x2wo6JpXixCWO/7qf6h0l8DJA
            //    Signing CA: RSA SHA256:NqLEgdYti0XjUkYjGyQv2Ddy1O5v2NZDZFRtlfESLIA (using rsa-sha2-512)
            // And we will authenticate (sign) with ssh-rsa (SHA-1)
            DoTest(PublicKeyAlgorithm.SshRsaCertV01OpenSSH, "Data.Key.OPENSSH.RSA.Encrypted.Aes.192.CTR.txt", "12345", "Data.Key.OPENSSH.RSA.Encrypted.Aes.192.CTR-cert.pub");
        }

        [TestMethod]
        public void SshRsaSha256Certificate()
        {
            // As above, but we will authenticate (sign) with rsa-sha2-256
            DoTest(PublicKeyAlgorithm.SshRsaCertV01OpenSSH, "Data.Key.OPENSSH.RSA.Encrypted.Aes.192.CTR.txt", "12345", "Data.Key.OPENSSH.RSA.Encrypted.Aes.192.CTR-cert.pub");
        }

        [TestMethod]
        public void Ecdsa256Certificate()
        {
            // ssh-keygen -L -f Key.OPENSSH.ECDSA.Encrypted.Aes.128.CTR-cert.pub
            //    Type: ecdsa-sha2-nistp256-cert-v01@openssh.com user certificate
            //    Public key: ECDSA-CERT SHA256:ufAaMwjTmKrjvt4CQiLPal1/HrmB2D7oL+H2lh/Om8c
            //    Signing CA: RSA SHA256:NqLEgdYti0XjUkYjGyQv2Ddy1O5v2NZDZFRtlfESLIA (using rsa-sha2-512)
            DoTest(PublicKeyAlgorithm.EcdsaSha2Nistp256CertV01OpenSSH, "Data.Key.OPENSSH.ECDSA.Encrypted.Aes.128.CTR.txt", "12345", "Data.Key.OPENSSH.ECDSA.Encrypted.Aes.128.CTR-cert.pub");
        }

        [TestMethod]
        public void Ecdsa384Certificate()
        {
            // ssh-keygen -L -f Key.OPENSSH.ECDSA384.Encrypted.Aes.256.GCM-cert.pub
            //    Type: ecdsa-sha2-nistp384-cert-v01@openssh.com user certificate
            //    Public key: ECDSA-CERT SHA256:wy4X47uddqD8nggcsGHG7Rcs0qcnh4r6NrdBGdh/8us
            //    Signing CA: RSA SHA256:NqLEgdYti0XjUkYjGyQv2Ddy1O5v2NZDZFRtlfESLIA (using rsa-sha2-256)
            DoTest(PublicKeyAlgorithm.EcdsaSha2Nistp384CertV01OpenSSH, "Data.Key.OPENSSH.ECDSA384.Encrypted.Aes.256.GCM.txt", "12345", "Data.Key.OPENSSH.ECDSA384.Encrypted.Aes.256.GCM-cert.pub");
        }

        [TestMethod]
        public void Ecdsa521Certificate()
        {
            // ssh-keygen -L -f Key.OPENSSH.ECDSA521.Encrypted.Aes.192.CBC-cert.pub
            //    Type: ecdsa-sha2-nistp521-cert-v01@openssh.com user certificate
            //    Public key: ECDSA-CERT SHA256:U3wBX0sSPYxso31gi1QPz7O+1eMOTb0LoOSOjWRwyYE
            //    Signing CA: ECDSA SHA256:r/t6I+bZQzN5BhSuntFSHDHlrnNHVM2lAo6gbvynG/4 (using ecdsa-sha2-nistp256)
            DoTest(PublicKeyAlgorithm.EcdsaSha2Nistp521CertV01OpenSSH, "Data.Key.OPENSSH.ECDSA521.Encrypted.Aes.192.CBC.txt", "12345", "Data.Key.OPENSSH.ECDSA521.Encrypted.Aes.192.CBC-cert.pub");
        }

        [TestMethod]
        public void Ed25519Certificate()
        {
            // ssh-keygen -L -f Key.OPENSSH.ED25519.Encrypted.ChaCha20.Poly1305-cert.pub
            //    Type: ssh-ed25519-cert-v01@openssh.com user certificate
            //    Public key: ED25519-CERT SHA256:gwO3eBcuPqChqg9B/kHsQo1/bYTAjaEZCanA7hqSuEg
            //    Signing CA: ECDSA SHA256:r/t6I+bZQzN5BhSuntFSHDHlrnNHVM2lAo6gbvynG/4 (using ecdsa-sha2-nistp256)
            DoTest(PublicKeyAlgorithm.SshEd25519CertV01OpenSSH, "Data.Key.OPENSSH.ED25519.Encrypted.ChaCha20.Poly1305.txt", "12345", "Data.Key.OPENSSH.ED25519.Encrypted.ChaCha20.Poly1305-cert.pub");
        }

        private void DoTest(PublicKeyAlgorithm publicKeyAlgorithm, string keyResource, string passPhrase = null, string certificateResource = null)
        {
            _remoteSshdConfig.ClearPublicKeyAcceptedAlgorithms()
                             .AddPublicKeyAcceptedAlgorithm(publicKeyAlgorithm)
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(CreatePrivateKeyAuthenticationMethod(keyResource, passPhrase, certificateResource));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
            }
        }

        private static PrivateKeyAuthenticationMethod CreatePrivateKeyAuthenticationMethod(string keyResource, string passPhrase, string certificateResource)
        {
            PrivateKeyFile privateKey;

            using (var keyStream = GetData(keyResource))
            {
                if (certificateResource is not null)
                {
                    using (var certificateStream = GetData(certificateResource))
                    {
                        privateKey = new PrivateKeyFile(keyStream, passPhrase, certificateStream);
                    }
                }
                else
                {
                    privateKey = new PrivateKeyFile(keyStream, passPhrase);
                }
            }

            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKey);
        }
    }
}
