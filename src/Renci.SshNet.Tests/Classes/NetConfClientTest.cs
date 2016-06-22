using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using System;
using System.Xml;

namespace Renci.SshNet.Tests.Classes
{
    //  TODO:   Please help with documentation here, as I don't know the details, specially for the methods not documented.
    /// <summary>
    ///
    /// </summary>
    [TestClass]
    public partial class NetConfClientTest : TestBase
    {
        /// <summary>
        ///A test for NetConfClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void NetConfClientConstructorTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(host, username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for NetConfClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void NetConfClientConstructorTest1()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(host, port, username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for NetConfClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void NetConfClientConstructorTest2()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(host, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for NetConfClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void NetConfClientConstructorTest3()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(host, port, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for NetConfClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void NetConfClientConstructorTest4()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(connectionInfo);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SendReceiveRpc
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SendReceiveRpcTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(connectionInfo); // TODO: Initialize to an appropriate value
            string xml = string.Empty; // TODO: Initialize to an appropriate value
            XmlDocument expected = null; // TODO: Initialize to an appropriate value
            XmlDocument actual;
            actual = target.SendReceiveRpc(xml);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SendReceiveRpc
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SendReceiveRpcTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(connectionInfo); // TODO: Initialize to an appropriate value
            XmlDocument rpc = null; // TODO: Initialize to an appropriate value
            XmlDocument expected = null; // TODO: Initialize to an appropriate value
            XmlDocument actual;
            actual = target.SendReceiveRpc(rpc);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SendCloseRpc
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SendCloseRpcTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(connectionInfo); // TODO: Initialize to an appropriate value
            XmlDocument expected = null; // TODO: Initialize to an appropriate value
            XmlDocument actual;
            actual = target.SendCloseRpc();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ServerCapabilities
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ServerCapabilitiesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(connectionInfo); // TODO: Initialize to an appropriate value
            XmlDocument actual;
            actual = target.ServerCapabilities;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OperationTimeout
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void OperationTimeoutTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(connectionInfo); // TODO: Initialize to an appropriate value
            TimeSpan expected = new TimeSpan(); // TODO: Initialize to an appropriate value
            TimeSpan actual;
            target.OperationTimeout = expected;
            actual = target.OperationTimeout;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ClientCapabilities
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ClientCapabilitiesTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(connectionInfo); // TODO: Initialize to an appropriate value
            XmlDocument actual;
            actual = target.ClientCapabilities;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AutomaticMessageIdHandling
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void AutomaticMessageIdHandlingTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            NetConfClient target = new NetConfClient(connectionInfo); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.AutomaticMessageIdHandling = expected;
            actual = target.AutomaticMessageIdHandling;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}