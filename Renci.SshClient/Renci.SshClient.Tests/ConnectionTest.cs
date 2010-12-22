using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Renci.SshClient.Tests.Properties;
using System.Security.Authentication;
using Renci.SshClient.Common;

namespace Renci.SshClient.Tests
{
    [TestClass]
    public class ConnectionTest
    {
        [TestMethod]
        public void Test_Connect_Using_Correct_Password()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SshAuthenticationException))]
        public void Test_Connect_Using_Invalid_Password()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Connect_Using_Rsa_Key_Without_PassPhrase()
        {
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITHOUT_PASS));
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, new PrivateKeyFile(keyFileStream)))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Connect_Using_RsaKey_With_PassPhrase()
        {
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITH_PASS));
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, new PrivateKeyFile(keyFileStream, Resources.PASSWORD)))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Connect_Using_DsaKey_Without_PassPhrase()
        {
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.DSA_KEY_WITHOUT_PASS));
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, new PrivateKeyFile(keyFileStream)))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Connect_Using_DsaKey_With_PassPhrase()
        {
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.DSA_KEY_WITH_PASS));
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, new PrivateKeyFile(keyFileStream, Resources.PASSWORD)))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SshAuthenticationException))]
        public void Test_Connect_Using_Invalid_PrivateKey()
        {
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.INVALID_KEY));
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, new PrivateKeyFile(keyFileStream)))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Connect_Using_Multiple_PrivateKeys()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME,
                new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(Resources.INVALID_KEY))),
                new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(Resources.DSA_KEY_WITH_PASS)), Resources.PASSWORD),
                new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITH_PASS)), Resources.PASSWORD),
                new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITHOUT_PASS))),
                new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(Resources.DSA_KEY_WITHOUT_PASS)))
                ))
            {
                client.Connect();
                client.Disconnect();
            }
        }


        [TestMethod]
        public void Test_Connect_Then_Reconnect()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                client.Disconnect();
                client.Connect();
                client.Disconnect();
            }
        }
    }
}
