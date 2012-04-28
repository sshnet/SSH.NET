using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Security
{
    [TestClass]
    public class TestKeyExchange
    {

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Key Exchange")]
        public void Test_KeyExchange_GroupExchange_Sha1_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.KeyExchangeAlgorithms.Clear();
            connectionInfo.KeyExchangeAlgorithms.Add("diffie-hellman-group-exchange-sha1", typeof(KeyExchangeDiffieHellmanGroupExchangeSha1));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Key Exchange")]
        public void Test_KeyExchange_Group14_Sha1_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.KeyExchangeAlgorithms.Clear();
            connectionInfo.KeyExchangeAlgorithms.Add("diffie-hellman-group14-sha1", typeof(KeyExchangeDiffieHellmanGroup14Sha1));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Key Exchange")]
        public void Test_KeyExchange_Group1_Sha1_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.KeyExchangeAlgorithms.Clear();
            connectionInfo.KeyExchangeAlgorithms.Add("diffie-hellman-group1-sha1", typeof(KeyExchangeDiffieHellmanGroup1Sha1));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Key Exchange")]
        public void Test_KeyExchange_GroupExchange_Sha256_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.KeyExchangeAlgorithms.Clear();
            connectionInfo.KeyExchangeAlgorithms.Add("diffie-hellman-group-exchange-sha256", typeof(KeyExchangeDiffieHellmanGroupExchangeSha256));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Key Exchange")]
        public void Test_KeyExchange_Rekeying()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                //  TODO:   Add test to test re-keying
                Assert.Inconclusive();
                client.Disconnect();
            }
        }
    }
}
