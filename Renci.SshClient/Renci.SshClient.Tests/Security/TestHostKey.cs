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
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.HostKeyAlgorithms.Clear();
                    e.HostKeyAlgorithms.Add("ssh-rsa", typeof(CryptoPublicKeyRsa).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_HostKey_SshDss_Connection()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.HostKeyAlgorithms.Clear();
                    e.HostKeyAlgorithms.Add("ssh-dss", typeof(CryptoPublicKeyDss).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
