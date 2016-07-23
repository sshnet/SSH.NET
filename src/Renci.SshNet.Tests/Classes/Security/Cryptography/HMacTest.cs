using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography
{
    /// <summary>
    /// Provides HMAC algorithm implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [TestClass]
    public class HMacTest : TestBase
    {
        [TestMethod]
        [TestCategory("integration")]
        public void Test_HMac_MD5_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-md5", new HashInfo(16 * 8, CryptoAbstraction.CreateHMACMD5));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_HMac_Sha1_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-sha1", new HashInfo(20 * 8, CryptoAbstraction.CreateHMACSHA1));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_HMac_MD5_96_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-md5", new HashInfo(16 * 8, key => CryptoAbstraction.CreateHMACMD5(key, 96)));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_HMac_Sha1_96_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-sha1", new HashInfo(20 * 8, key => CryptoAbstraction.CreateHMACSHA1(key, 96)));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_HMac_Sha256_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-sha2-256", new HashInfo(32 * 8, CryptoAbstraction.CreateHMACSHA256));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_HMac_Sha256_96_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-sha2-256-96", new HashInfo(32 * 8, (key) => CryptoAbstraction.CreateHMACSHA256(key, 96)));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_HMac_RIPEMD160_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-ripemd160", new HashInfo(160, CryptoAbstraction.CreateHMACRIPEMD160));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_HMac_RIPEMD160_OPENSSH_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-ripemd160@openssh.com", new HashInfo(160, CryptoAbstraction.CreateHMACRIPEMD160));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }
    }
}