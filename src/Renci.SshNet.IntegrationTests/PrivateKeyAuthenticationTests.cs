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
        public void Ecdsa256()
        {
            _remoteSshdConfig.AddPublicKeyAcceptedAlgorithms(PublicKeyAlgorithm.EcdsaSha2Nistp256)
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(CreatePrivateKeyAuthenticationMethod("key_ecdsa_256_openssh"));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        public void Ecdsa384()
        {
            _remoteSshdConfig.AddPublicKeyAcceptedAlgorithms(PublicKeyAlgorithm.EcdsaSha2Nistp384)
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(CreatePrivateKeyAuthenticationMethod("key_ecdsa_384_openssh"));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        public void EcdsaA521()
        {
            _remoteSshdConfig.AddPublicKeyAcceptedAlgorithms(PublicKeyAlgorithm.EcdsaSha2Nistp521)
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(CreatePrivateKeyAuthenticationMethod("key_ecdsa_521_openssh"));

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
