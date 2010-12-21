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
    public class TestCipher
    {
        [TestMethod]
        public void Test_Cipher_TripleDES_Connection()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.Encryptions.Clear();
                    e.Encryptions.Add("3des-cbc", typeof(CipherTripleDES).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_AES128CBC_Connection()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.Encryptions.Clear();
                    e.Encryptions.Add("aes128-cbc", typeof(CipherAES128CBC).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_AES192CBC_Connection()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.Encryptions.Clear();
                    e.Encryptions.Add("aes192-cbc", typeof(CipherAES192CBC).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_AES256CBC_Connection()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connecting += delegate(object sender, Common.ConnectingEventArgs e)
                {
                    e.Encryptions.Clear();
                    e.Encryptions.Add("aes256-cbc", typeof(CipherAES256CBC).AssemblyQualifiedName);
                };
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_TripleDES_Algorithm()
        {
            //var cipher = new CipherTripleDES();
            //cipher.Init();
            //cipher.Encrypt();
            //cipher.Decrypt();
        }

    }
}
