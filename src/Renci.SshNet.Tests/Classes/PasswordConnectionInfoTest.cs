using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Net;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides connection information when password authentication method is used
    /// </summary>
    [TestClass]
    public class PasswordConnectionInfoTest : TestBase
    {
        [TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [TestCategory("integration")]
        public void Test_PasswordConnectionInfo()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            #region Example PasswordConnectionInfo
            var connectionInfo = new PasswordConnectionInfo(host, username, password);
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                //  Do something here
                client.Disconnect();
            }
            #endregion

            Assert.AreEqual(connectionInfo.Host, Resources.HOST);
            Assert.AreEqual(connectionInfo.Username, Resources.USERNAME);
        }

        [TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [TestCategory("integration")]
        public void Test_PasswordConnectionInfo_PasswordExpired()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            #region Example PasswordConnectionInfo PasswordExpired
            var connectionInfo = new PasswordConnectionInfo("host", "username", "password");
            var encoding = SshData.Ascii;
            connectionInfo.PasswordExpired += delegate(object sender, AuthenticationPasswordChangeEventArgs e)
            {
                e.NewPassword = encoding.GetBytes("123456");
            };

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();

                client.Disconnect();
            }
            #endregion

            Assert.Inconclusive();
        }
        [TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [TestCategory("integration")]
        public void Test_PasswordConnectionInfo_AuthenticationBanner()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            #region Example PasswordConnectionInfo AuthenticationBanner
            var connectionInfo = new PasswordConnectionInfo(host, username, password);
            connectionInfo.AuthenticationBanner += delegate(object sender, AuthenticationBannerEventArgs e)
            {
                Console.WriteLine(e.BannerMessage);
            };
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                //  Do something here
                client.Disconnect();
            }
            #endregion

            Assert.AreEqual(connectionInfo.Host, Resources.HOST);
            Assert.AreEqual(connectionInfo.Username, Resources.USERNAME);
        }


        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        public void Test_ConnectionInfo_Host_Is_Null()
        {
            try
            {
                new PasswordConnectionInfo(null, Resources.USERNAME, Resources.PASSWORD);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("host", ex.ParamName);
            }

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
        [Owner("Kenneth_aa")]
        [Description("Test connect to remote server via a SOCKS4 proxy server.")]
        [TestCategory("Proxy")]
        [TestCategory("integration")]
        public void Test_Ssh_Connect_Via_Socks4()
        {
            var connInfo = new PasswordConnectionInfo(Resources.HOST, Resources.USERNAME, Resources.PASSWORD, ProxyTypes.Socks4, Resources.PROXY_HOST, int.Parse(Resources.PROXY_PORT));
            using (var client = new SshClient(connInfo))
            {
                client.Connect();

                var ret = client.RunCommand("ls -la");

                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("Kenneth_aa")]
        [Description("Test connect to remote server via a TCP SOCKS5 proxy server.")]
        [TestCategory("Proxy")]
        [TestCategory("integration")]
        public void Test_Ssh_Connect_Via_TcpSocks5()
        {
            var connInfo = new PasswordConnectionInfo(Resources.HOST, Resources.USERNAME, Resources.PASSWORD, ProxyTypes.Socks5, Resources.PROXY_HOST, int.Parse(Resources.PROXY_PORT));
            using (var client = new SshClient(connInfo))
            {
                client.Connect();

                var ret = client.RunCommand("ls -la");
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("Kenneth_aa")]
        [Description("Test connect to remote server via a HTTP proxy server.")]
        [TestCategory("Proxy")]
        [TestCategory("integration")]
        public void Test_Ssh_Connect_Via_HttpProxy()
        {
            var connInfo = new PasswordConnectionInfo(Resources.HOST, Resources.USERNAME, Resources.PASSWORD, ProxyTypes.Http, Resources.PROXY_HOST, int.Parse(Resources.PROXY_PORT));
            using (var client = new SshClient(connInfo))
            {
                client.Connect();

                var ret = client.RunCommand("ls -la");

                client.Disconnect();
            }
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DisposeTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest1()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest2()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort, proxyUsername);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest3()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest4()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password, proxyType, proxyHost, proxyPort, proxyUsername);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest5()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password, proxyType, proxyHost, proxyPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest6()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest7()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            byte[] password = null; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest8()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            string proxyPassword = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest9()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort, proxyUsername);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest10()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password, proxyType, proxyHost, proxyPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest11()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            string proxyUsername = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password, proxyType, proxyHost, proxyPort, proxyUsername);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest12()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            ProxyTypes proxyType = new ProxyTypes(); // TODO: Initialize to an appropriate value
            string proxyHost = string.Empty; // TODO: Initialize to an appropriate value
            int proxyPort = 0; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password, proxyType, proxyHost, proxyPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest13()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, port, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for PasswordConnectionInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PasswordConnectionInfoConstructorTest14()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            PasswordConnectionInfo target = new PasswordConnectionInfo(host, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

    }
}