using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;
using System.IO;
using System.Security.Cryptography;
using Renci.SshNet.Sftp;
using System.Threading;
using System.Diagnostics;

namespace Renci.SshNet.Tests.SshClientTests
{
    [TestClass]
    public class ProxyTest
    {
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
                Debug.WriteLine(ret.Result, "Command result:");

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
                Debug.WriteLine(ret.Result, "Command result:");

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
                Debug.WriteLine(ret.Result, "Command result:");

                client.Disconnect();
            }
        }
    }
}
