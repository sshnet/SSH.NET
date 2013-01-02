using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var connectionInfo = new PasswordConnectionInfo(" ", Resources.USERNAME, Resources.PASSWORD);
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
    }
}