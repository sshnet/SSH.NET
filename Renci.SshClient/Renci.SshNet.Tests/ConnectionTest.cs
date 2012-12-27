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
        [TestCategory("Authentication")]
        public void Test_Connect_Using_Correct_Password()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Authentication")]
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
        [TestCategory("Authentication")]
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
        [TestCategory("Authentication")]
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
        [TestCategory("Authentication")]
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
        [TestCategory("Authentication")]
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
        [TestCategory("Authentication")]
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
        [TestCategory("Authentication")]
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
        [TestCategory("Authentication")]
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
        [TestCategory("Authentication")]
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
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Host_Is_Null()
        {
            var connectionInfo = new PasswordConnectionInfo(null, Resources.USERNAME, Resources.PASSWORD);
        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Username_Is_Null()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, null, Resources.PASSWORD);
        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_ConnectionInfo_Password_Is_Null()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, Resources.USERNAME, (string)null);
        }

        [TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [Description("Test passing whitespace to host parameter.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Host_Is_Whitespace()
        {
            var connectionInfo = new PasswordConnectionInfo(" ", Resources.USERNAME,Resources.PASSWORD);
        }

        [TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [Description("Test passing whitespace to username parameter.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Username_Is_Whitespace()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, " ", Resources.PASSWORD);
        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_SmallPortNumber()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MinPort - 1, Resources.USERNAME, Resources.PASSWORD);
        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_BigPortNumber()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MaxPort + 1, Resources.USERNAME, Resources.PASSWORD);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass null as proxy host.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_ProxyHost_Null()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, null, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass String.Empty as proxy host.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_ProxyHost_Empty()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, string.Empty, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too large proxy port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_ProxyPort_Large()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.MaxValue, Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too small proxy port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_ProxyPort_Small()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.MinValue, Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass a valid proxy port.")]
        [Owner("Kenneth_aa")]
        public void Test_ConnectionInfo_ProxyPort_Valid()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass null as host.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Host_Null()
        {
            new ConnectionInfo(null, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass String.Empty as host.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Host_Empty()
        {
            new ConnectionInfo(string.Empty, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass a valid host.")]
        [Owner("Kenneth_aa")]
        public void Test_ConnectionInfo_Host_Valid()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too large port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_Port_Large()
        {
            new ConnectionInfo(Resources.HOST, int.MaxValue, Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass too small port.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_Port_Small()
        {
            new ConnectionInfo(Resources.HOST, int.MinValue, Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass a valid port.")]
        [Owner("Kenneth_aa")]
        public void Test_ConnectionInfo_Port_Valid()
        {
            new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        [Description("Pass null as session.")]
        [Owner("Kenneth_aa")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_ConnectionInfo_Authenticate_Null()
        {
            var ret = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
            ret.Authenticate(null);
        }

        [TestMethod]
        [WorkItem(1140)]
        [TestCategory("BaseClient")]
        [Description("Test whether IsConnected is false after disconnect.")]
        [Owner("Kenneth_aa")]
        public void Test_BaseClient_IsConnected_True_After_Disconnect()
        {
            // 2012-04-29 - Kenneth_aa
            // The problem with this test, is that after SSH Net calls .Disconnect(), the library doesn't wait
            // for the server to confirm disconnect before IsConnected is checked. And now I'm not mentioning
            // anything about Socket's either.

            var connectionInfo = new PasswordAuthenticationMethod(Resources.USERNAME, Resources.PASSWORD);

            using (SftpClient client = new SftpClient(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                Assert.AreEqual<bool>(true, client.IsConnected, "IsConnected is not true after Connect() was called.");

                client.Disconnect();

                Assert.AreEqual<bool>(false, client.IsConnected, "IsConnected is true after Disconnect() was called.");
            }
        }
    }
}
