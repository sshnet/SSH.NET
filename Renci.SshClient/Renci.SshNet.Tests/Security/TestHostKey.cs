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
    public class TestHostKey
    {
        [TestMethod]
        public void Test_HostKey_SshRsa_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.HostKeyAlgorithms.Clear();
            connectionInfo.HostKeyAlgorithms.Add("ssh-rsa", (data) => { return new KeyHostAlgorithm("ssh-rsa", new RsaKey(), data); });
            
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
            connectionInfo.HostKeyAlgorithms.Add("ssh-dss", (data) => { return new KeyHostAlgorithm("ssh-dss", new DsaKey(), data); });

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
