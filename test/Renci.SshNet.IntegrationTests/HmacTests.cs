using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class HmacTests : IntegrationTestBase
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
        public void HmacSha1()
        {
            DoTest(MessageAuthenticationCodeAlgorithm.HmacSha1);
        }

        [TestMethod]
        public void HmacSha2_256()
        {
            DoTest(MessageAuthenticationCodeAlgorithm.HmacSha2_256);
        }

        [TestMethod]
        public void HmacSha2_512()
        {
            DoTest(MessageAuthenticationCodeAlgorithm.HmacSha2_512);
        }


        [TestMethod]
        public void HmacSha1_Etm()
        {
            DoTest(MessageAuthenticationCodeAlgorithm.HmacSha1Etm);
        }

        [TestMethod]
        public void HmacSha2_256_Etm()
        {
            DoTest(MessageAuthenticationCodeAlgorithm.HmacSha2_256_Etm);
        }

        [TestMethod]
        public void HmacSha2_512_Etm()
        {
            DoTest(MessageAuthenticationCodeAlgorithm.HmacSha2_512_Etm);
        }

        private void DoTest(MessageAuthenticationCodeAlgorithm macAlgorithm)
        {
            _remoteSshdConfig.ClearMessageAuthenticationCodeAlgorithms()
                             .AddMessageAuthenticationCodeAlgorithm(macAlgorithm)
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
