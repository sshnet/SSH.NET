using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public sealed class CipherTests : IntegrationTestBase
    {
        private LinuxVMConnectionFactory _connectionInfoFactory;
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
        public void TripledesCbc()
        {
            DoTest(Cipher.TripledesCbc);
        }

        [TestMethod]
        public void Aes128Cbc()
        {
            DoTest(Cipher.Aes128Cbc);
        }

        [TestMethod]
        public void Aes192Cbc()
        {
            DoTest(Cipher.Aes192Cbc);
        }

        [TestMethod]
        public void Aes256Cbc()
        {
            DoTest(Cipher.Aes256Cbc);
        }

        [TestMethod]
        public void Aes128Ctr()
        {
            DoTest(Cipher.Aes128Ctr);
        }

        [TestMethod]
        public void Aes192Ctr()
        {
            DoTest(Cipher.Aes192Ctr);
        }

        [TestMethod]
        public void Aes256Ctr()
        {
            DoTest(Cipher.Aes256Ctr);
        }

        private void DoTest(Cipher cipher)
        {
            _remoteSshdConfig.ClearCiphers()
                             .AddCipher(cipher)
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
