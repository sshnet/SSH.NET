using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    [TestClass]
    public class AesCipherTests : IntegrationTestBase
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
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        public void Test_Cipher_AEes128CBC_Connection()
        {
            _remoteSshdConfig.AddCipher(Cipher.Aes128Cbc)
                             .Update()
                             .Restart();

            var connectionInfo = new PasswordConnectionInfo(SshServerHostName, SshServerPort, User.UserName, User.Password);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes128-cbc", new CipherInfo(128, (key, iv) => { return new AesCipher(key, new CbcCipherMode(iv), null); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        public void Test_Cipher_Aes192CBC_Connection()
        {
            _remoteSshdConfig.AddCipher(Cipher.Aes192Cbc)
                             .Update()
                             .Restart();

            var connectionInfo = new PasswordConnectionInfo(SshServerHostName, SshServerPort, User.UserName, User.Password);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes192-cbc", new CipherInfo(192, (key, iv) => { return new AesCipher(key, new CbcCipherMode(iv), null); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        public void Test_Cipher_Aes256CBC_Connection()
        {
            _remoteSshdConfig.AddCipher(Cipher.Aes256Cbc)
                             .Update()
                             .Restart();

            var connectionInfo = new PasswordConnectionInfo(SshServerHostName, SshServerPort, User.UserName, User.Password);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes256-cbc", new CipherInfo(256, (key, iv) => { return new AesCipher(key, new CbcCipherMode(iv), null); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        public void Test_Cipher_Aes128CTR_Connection()
        {
            _remoteSshdConfig.AddCipher(Cipher.Aes128Ctr)
                             .Update()
                             .Restart();

            var connectionInfo = new PasswordConnectionInfo(SshServerHostName, SshServerPort, User.UserName, User.Password);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes128-ctr", new CipherInfo(128, (key, iv) => { return new AesCipher(key, new CtrCipherMode(iv), null); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        public void Test_Cipher_Aes192CTR_Connection()
        {
            _remoteSshdConfig.AddCipher(Cipher.Aes192Ctr)
                             .Update()
                             .Restart();

            var connectionInfo = new PasswordConnectionInfo(SshServerHostName, SshServerPort, User.UserName, User.Password);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes192-ctr", new CipherInfo(192, (key, iv) => { return new AesCipher(key, new CtrCipherMode(iv), null); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        public void Test_Cipher_Aes256CTR_Connection()
        {
            _remoteSshdConfig.AddCipher(Cipher.Aes256Ctr)
                             .Update()
                             .Restart();

            var connectionInfo = new PasswordConnectionInfo(SshServerHostName, SshServerPort, User.UserName, User.Password);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes256-ctr", new CipherInfo(256, (key, iv) => { return new AesCipher(key, new CtrCipherMode(iv), null); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
