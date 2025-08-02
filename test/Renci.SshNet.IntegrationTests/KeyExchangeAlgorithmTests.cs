using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class KeyExchangeAlgorithmTests : IntegrationTestBase
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
        public void MLKem768X25519Sha256()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.MLKem768X25519Sha256)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void SNtruP761X25519Sha512()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.SNtruP761X25519Sha512)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void SNtruP761X25519Sha512OpenSsh()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.SNtruP761X25519Sha512OpenSsh)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Curve25519Sha256()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.Curve25519Sha256)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Curve25519Sha256Libssh()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.Curve25519Sha256Libssh)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void DiffieHellmanGroup1Sha1()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.DiffieHellmanGroup1Sha1)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void DiffieHellmanGroup14Sha1()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.DiffieHellmanGroup14Sha1)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void DiffieHellmanGroup14Sha256()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.DiffieHellmanGroup14Sha256)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void DiffieHellmanGroup16Sha512()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.DiffieHellmanGroup16Sha512)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void DiffieHellmanGroup18Sha512()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.DiffieHellmanGroup18Sha512)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void DiffieHellmanGroupExchangeSha1()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.DiffieHellmanGroupExchangeSha1)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void DiffieHellmanGroupExchangeSha256()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.DiffieHellmanGroupExchangeSha256)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void EcdhSha2Nistp256()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.EcdhSha2Nistp256)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void EcdhSha2Nistp384()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.EcdhSha2Nistp384)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void EcdhSha2Nistp521()
        {
            _remoteSshdConfig.ClearKeyExchangeAlgorithms()
                             .AddKeyExchangeAlgorithm(KeyExchangeAlgorithm.EcdhSha2Nistp521)
                             .Update()
                             .Restart();

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
