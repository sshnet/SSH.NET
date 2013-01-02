using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Net;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for remote port forwarding
    /// </summary>
    [TestClass]
    public partial class ForwardedPortRemoteTest : TestBase
    {
        [TestMethod]
        [Description("Test passing null to AddForwardedPort hosts (remote).")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_AddForwardedPort_Remote_Hosts_Are_Null()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var port1 = new ForwardedPortRemote(null, 8080, null, 80);
                client.AddForwardedPort(port1);
                client.Disconnect();
            }
        }

        [TestMethod]
        [Description("Test passing invalid port numbers to AddForwardedPort.")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_AddForwardedPort_Invalid_PortNumber()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var port1 = new ForwardedPortRemote("localhost", IPEndPoint.MaxPort + 1, "www.renci.org", IPEndPoint.MaxPort + 1);
                client.AddForwardedPort(port1);
                client.Disconnect();
            }
        }
    }
}