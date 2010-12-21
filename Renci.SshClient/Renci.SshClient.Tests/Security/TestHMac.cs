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
    public class TestHMac
    {
        [TestMethod]
        public void Test_HMac_MD5_Connection()
        {            
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.HmacAlgorithms.Clear();
                    e.HmacAlgorithms.Add("hmac-md5", typeof(HMacMD5).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_HMac_Sha1_Connection()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.HmacAlgorithms.Clear();
                    e.HmacAlgorithms.Add("hmac-sha1", typeof(HMacSha1).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
