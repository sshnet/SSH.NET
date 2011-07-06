using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Security
{
    [TestClass]
    public class TestCipher
    {
        [TestMethod]
        public void Test_Cipher_TripleDESCBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("3des-cbc", typeof(CipherTripleDes192Cbc));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_AEes128CBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes128-cbc", typeof(CipherAes128Cbc));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_Aes192CBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes192-cbc", typeof(CipherAes192Cbc));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_Aes256CBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes256-cbc", typeof(CipherAes256Cbc));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_Aes128CTR_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes128-ctr", typeof(CipherAes128Ctr));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_Aes192CTR_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes192-ctr", typeof(CipherAes192Ctr));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_Aes256CTR_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes256-ctr", typeof(CipherAes256Ctr));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_BlowfishCBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("blowfish-cbc", typeof(CipherBlowfish));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Cipher_Cast128CBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, 22, Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("cast128-cbc", typeof(CipherCast128Cbc));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
