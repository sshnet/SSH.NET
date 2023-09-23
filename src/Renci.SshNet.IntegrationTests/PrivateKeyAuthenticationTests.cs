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
            DoTest(PublicKeyAlgorithm.SshDss, "id_dsa");
        }

        [TestMethod]
        public void SshRsa()
        {
            DoTest(PublicKeyAlgorithm.SshRsa, "id_rsa");
        }

        [TestMethod]
        public void SshRsaSha256()
        {
            DoTest(PublicKeyAlgorithm.RsaSha2256, "id_rsa");
        }

        [TestMethod]
        public void SshRsaSha512()
        {
            DoTest(PublicKeyAlgorithm.RsaSha2512, "id_rsa");
        }

        [TestMethod]
        public void Ecdsa256()
        {
            DoTest(PublicKeyAlgorithm.EcdsaSha2Nistp256, "key_ecdsa_256_openssh");
        }

        [TestMethod]
        public void Ecdsa384()
        {
            DoTest(PublicKeyAlgorithm.EcdsaSha2Nistp384, "key_ecdsa_384_openssh");
        }

        [TestMethod]
        public void Ecdsa521()
        {
            DoTest(PublicKeyAlgorithm.EcdsaSha2Nistp521, "key_ecdsa_521_openssh");
        }

        [TestMethod]
        public void Ed25519()
        {
            DoTest(PublicKeyAlgorithm.SshEd25519, "key_ed25519_openssh");
        }

        private void DoTest(PublicKeyAlgorithm publicKeyAlgorithm, string keyResource)
        {
            _remoteSshdConfig.ClearPublicKeyAcceptedAlgorithms()
                             .AddPublicKeyAcceptedAlgorithms(publicKeyAlgorithm)
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(CreatePrivateKeyAuthenticationMethod(keyResource));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
            }
        }

        private PrivateKeyAuthenticationMethod CreatePrivateKeyAuthenticationMethod(string keyResource)
        {
            var privateKey = CreatePrivateKeyFromManifestResource("Renci.SshNet.IntegrationTests.resources.client." + keyResource);
            return new PrivateKeyAuthenticationMethod(Users.Regular.UserName, privateKey);
        }

        private PrivateKeyFile CreatePrivateKeyFromManifestResource(string resourceName)
        {
            using (var stream = GetManifestResourceStream(resourceName))
            {
                return new PrivateKeyFile(stream);
            }
        }
    }
}
