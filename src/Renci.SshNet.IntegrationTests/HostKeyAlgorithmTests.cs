using Renci.SshNet.Common;
using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class HostKeyAlgorithmTests : IntegrationTestBase
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
        [Ignore] // No longer supported in recent versions of OpenSSH
        public void SshDsa()
        {
            _remoteSshdConfig.ClearHostKeyAlgorithms()
                             .AddHostKeyAlgorithm(HostKeyAlgorithm.SshDsa)
                             .ClearHostKeyFiles()
                             .AddHostKeyFile(HostKeyFile.Dsa.FilePath)
                             .Update()
                             .Restart();

            HostKeyEventArgs hostKeyEventsArgs = null;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) => hostKeyEventsArgs = e;
                client.Connect();
                client.Disconnect();
            }

            Assert.IsNotNull(hostKeyEventsArgs);
            Assert.AreEqual(HostKeyFile.Dsa.KeyName, hostKeyEventsArgs.HostKeyName);
            Assert.AreEqual(1024, hostKeyEventsArgs.KeyLength);
            Assert.IsTrue(hostKeyEventsArgs.FingerPrint.SequenceEqual(HostKeyFile.Dsa.FingerPrint));
        }

        [TestMethod]
        public void SshRsa()
        {
            _remoteSshdConfig.ClearHostKeyAlgorithms()
                             .AddHostKeyAlgorithm(HostKeyAlgorithm.SshRsa)
                             .Update()
                             .Restart();

            HostKeyEventArgs hostKeyEventsArgs = null;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) => hostKeyEventsArgs = e;
                client.Connect();
                client.Disconnect();
            }

            Assert.IsNotNull(hostKeyEventsArgs);
            Assert.AreEqual(HostKeyFile.Rsa.KeyName, hostKeyEventsArgs.HostKeyName);
            Assert.AreEqual(3072, hostKeyEventsArgs.KeyLength);
            Assert.IsTrue(hostKeyEventsArgs.FingerPrint.SequenceEqual(HostKeyFile.Rsa.FingerPrint));
        }

        [TestMethod]
        public void SshEd25519()
        {
            _remoteSshdConfig.ClearHostKeyAlgorithms()
                             .AddHostKeyAlgorithm(HostKeyAlgorithm.SshEd25519)
                             .ClearHostKeyFiles()
                             .AddHostKeyFile(HostKeyFile.Ed25519.FilePath)
                             .Update()
                             .Restart();

            HostKeyEventArgs hostKeyEventsArgs = null;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) => hostKeyEventsArgs = e;
                client.Connect();
                client.Disconnect();
            }

            Assert.IsNotNull(hostKeyEventsArgs);
            Assert.AreEqual(HostKeyFile.Ed25519.KeyName, hostKeyEventsArgs.HostKeyName);
            Assert.AreEqual(256, hostKeyEventsArgs.KeyLength);
            Assert.IsTrue(hostKeyEventsArgs.FingerPrint.SequenceEqual(HostKeyFile.Ed25519.FingerPrint));
        }

        private void Client_HostKeyReceived(object sender, HostKeyEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
