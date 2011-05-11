using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshClient.Security;
using Renci.SshClient.Tests.Properties;

namespace Renci.SshClient.Tests.Security
{
    [TestClass]
    public class TestHostKey
    {
        [TestMethod]
        public void Test_HostKey_SshRsa_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HostKeyAlgorithms.Clear();
            connectionInfo.HostKeyAlgorithms.Add("ssh-rsa", typeof(CryptoPublicKeyRsa));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_HostKey_SshDss_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HostKeyAlgorithms.Clear();
            connectionInfo.HostKeyAlgorithms.Add("ssh-dss", typeof(CryptoPublicKeyDss));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
