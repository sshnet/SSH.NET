using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Renci.SshNet.Tests.Properties;
using System.Security.Authentication;
using Renci.SshNet.Common;
using System.Net;

namespace Renci.SshNet.Tests
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
        [ExpectedException(typeof(SshPassPhraseNullOrEmptyException))]
        public void Test_Connect_Using_Key_With_Empty_PassPhrase()
        {
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITH_PASS));
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, new PrivateKeyFile(keyFileStream, null)))
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

		[WorkItem(703), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_ConnectionInfo_NullHost()
        {
            var connectionInfo = new PasswordConnectionInfo(null, null, null);
        }

		[TestMethod]
		[Description("Test passing whitespace to host parameter.")]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_ConnectionInfo_Host_Is_Whitespace()
		{
			var connectionInfo = new PasswordConnectionInfo(" ", Resources.USERNAME,Resources.PASSWORD);
		}

		[TestMethod]
		[Description("Test passing whitespace to username parameter.")]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_ConnectionInfo_Username_Is_Whitespace()
		{
			var connectionInfo = new PasswordConnectionInfo(Resources.HOST, " ", Resources.PASSWORD);
		}

		[TestMethod]
		[Description("Test passing whitespace to password parameter.")]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_ConnectionInfo_Password_Is_Whitespace()
		{
			var connectionInfo = new PasswordConnectionInfo(Resources.HOST, Resources.USERNAME, " ");
		}

		[WorkItem(703), TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_SmallPortNumber()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MinPort - 1, null, null);
        }

		[WorkItem(703), TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_BigPortNumber()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MaxPort + 1, null, null);
        }

    }
}
