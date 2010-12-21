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
    public class TestKeyExchange
    {

        [TestMethod]
        public void Test_KeyExchange_GroupExchange_Sha1_Connection()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.KeyExchangeAlgorithms.Clear();
                    e.KeyExchangeAlgorithms.Add("diffie-hellman-group-exchange-sha1", typeof(KeyExchangeDiffieHellmanGroupExchangeSha1).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_KeyExchange_Group14_Sha1_Connection()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.KeyExchangeAlgorithms.Clear();
                    e.KeyExchangeAlgorithms.Add("diffie-hellman-group14-sha1", typeof(KeyExchangeDiffieHellmanGroup14Sha1).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_KeyExchange_Group1_Sha1_Connection()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.KeyExchangeAlgorithms.Clear();
                    e.KeyExchangeAlgorithms.Add("diffie-hellman-group1-sha1", typeof(KeyExchangeDiffieHellmanGroup1Sha1).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_KeyExchange_GroupExchange_Sha256_Connection()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.KeyExchangeAlgorithms.Clear();
                    e.KeyExchangeAlgorithms.Add("diffie-hellman-group-exchange-sha256", typeof(KeyExchangeDiffieHellmanGroupExchangeSha256).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }

        //  TODO:   Add test to test re-keying
    }
}
