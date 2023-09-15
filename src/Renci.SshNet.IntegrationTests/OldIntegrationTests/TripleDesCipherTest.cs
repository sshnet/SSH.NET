using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Implements 3DES cipher algorithm.
    /// </summary>
    [TestClass]
    public class TripleDesCipherTest : IntegrationTestBase
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
        public void Test_Cipher_TripleDESCBC_Connection()
        {
            _remoteSshdConfig.AddCipher(Cipher.TripledesCbc)
                             .Update()
                             .Restart();

            var connectionInfo = new PasswordConnectionInfo(SshServerHostName, SshServerPort, User.UserName, User.Password);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("3des-cbc", new CipherInfo(192, (key, iv) => { return new TripleDesCipher(key, new CbcCipherMode(iv), null); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
