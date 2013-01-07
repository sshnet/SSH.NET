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

        /// <summary>
        ///A test for Stop
        ///</summary>
        [TestMethod()]
        public void StopTest()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            string host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortRemote target = new ForwardedPortRemote(boundPort, host, port); // TODO: Initialize to an appropriate value
            target.Stop();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Start
        ///</summary>
        [TestMethod()]
        public void StartTest()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            string host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortRemote target = new ForwardedPortRemote(boundPort, host, port); // TODO: Initialize to an appropriate value
            target.Start();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            string host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortRemote target = new ForwardedPortRemote(boundPort, host, port); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ForwardedPortRemote Constructor
        ///</summary>
        [TestMethod()]
        public void ForwardedPortRemoteConstructorTest()
        {
            string boundHost = string.Empty; // TODO: Initialize to an appropriate value
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            string host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortRemote target = new ForwardedPortRemote(boundHost, boundPort, host, port);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ForwardedPortRemote Constructor
        ///</summary>
        [TestMethod()]
        public void ForwardedPortRemoteConstructorTest1()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            string host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortRemote target = new ForwardedPortRemote(boundPort, host, port);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}