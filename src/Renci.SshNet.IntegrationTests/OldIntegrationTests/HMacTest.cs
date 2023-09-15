using Renci.SshNet.Abstractions;
using Renci.SshNet.IntegrationTests.Common;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    [TestClass]
    public class HMacTest : IntegrationTestBase
    {
        private IConnectionInfoFactory _adminConnectionInfoFactory;
        private RemoteSshdConfig _remoteSshdConfig;

        [TestInitialize]
        public void SetUp()
        {
            _adminConnectionInfoFactory = new LinuxAdminConnectionFactory(SshServerHostName, SshServerPort);
            _remoteSshdConfig = new RemoteSshd(_adminConnectionInfoFactory).OpenConfig();
        }

        [TestCleanup]
        public void TearDown()
        {
            _remoteSshdConfig?.Reset();
        }

        [TestMethod]
        public void Test_HMac_Sha1_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(SshServerHostName, SshServerPort, User.UserName, User.Password);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-sha1", new HashInfo(20 * 8, CryptoAbstraction.CreateHMACSHA1));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_HMac_Sha256_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(SshServerHostName, SshServerPort, User.UserName, User.Password);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-sha2-256", new HashInfo(32 * 8, CryptoAbstraction.CreateHMACSHA256));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_HMac_Sha2_512_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(SshServerHostName, SshServerPort, User.UserName, User.Password);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-sha2-512", new HashInfo(64 * 8, CryptoAbstraction.CreateHMACSHA512));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
