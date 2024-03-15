using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    internal class PrivateKeyAuthenticationTests : TestBase
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

        private void DoTest(PublicKeyAlgorithm publicKeyAlgorithm, string keyResource, string passPhrase = null)
        {
            _remoteSshdConfig.ClearPublicKeyAcceptedAlgorithms()
                             .AddPublicKeyAcceptedAlgorithm(publicKeyAlgorithm)
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(CreatePrivateKeyAuthenticationMethod(keyResource, passPhrase));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
            }
        }

        private static PrivateKeyAuthenticationMethod CreatePrivateKeyAuthenticationMethod(string keyResource, string passPhrase)
        {
            using (var stream = GetData(keyResource))
            {
                return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, new PrivateKeyFile(stream, passPhrase));
            }
        }
    }
}
