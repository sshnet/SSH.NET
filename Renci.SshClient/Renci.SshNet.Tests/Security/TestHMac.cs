using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security;
using Renci.SshNet.Tests.Properties;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Tests.Security
{
    [TestClass]
    public class TestHMac
    {
        [TestMethod]
        public void Test_HMac_MD5_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-md5", (key) => { return new HMac<MD5Hash>(key.Take(16).ToArray());});

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_HMac_Sha1_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HmacAlgorithms.Clear();
            connectionInfo.HmacAlgorithms.Add("hmac-sha1", (key) => { return new HMac<SHA1Hash>(key.Take(20).ToArray()); });

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
