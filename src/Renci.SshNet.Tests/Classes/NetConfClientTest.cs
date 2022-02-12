using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using System;
using System.Xml;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class NetConfClientTest : TestBase
    {
        private Random _random;

        [TestInitialize]
        public void SetUp()
        {
            _random = new Random();
        }

        [TestMethod]
        public void OperationTimeout_Default()
        {
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new NetConfClient(connectionInfo);

            var actual = target.OperationTimeout;

            Assert.AreEqual(TimeSpan.FromMilliseconds(-1), actual);
        }

        [TestMethod]
        public void OperationTimeout_InsideLimits()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(_random.Next(0, int.MaxValue - 1));
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new NetConfClient(connectionInfo)
                {
                    OperationTimeout = operationTimeout
                };

            var actual = target.OperationTimeout;

            Assert.AreEqual(operationTimeout, actual);
        }

        [TestMethod]
        public void OperationTimeout_LowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(-1);
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new NetConfClient(connectionInfo)
                {
                    OperationTimeout = operationTimeout
            };

            var actual = target.OperationTimeout;

            Assert.AreEqual(operationTimeout, actual);
        }

        [TestMethod]
        public void OperationTimeout_UpperLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(int.MaxValue);
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new NetConfClient(connectionInfo)
                {
                    OperationTimeout = operationTimeout
                };

            var actual = target.OperationTimeout;

            Assert.AreEqual(operationTimeout, actual);
        }

        [TestMethod]
        public void OperationTimeout_LessThanLowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(-2);
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new NetConfClient(connectionInfo);

            try
            {
                target.OperationTimeout = operationTimeout;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("The timeout must represent a value between -1 and Int32.MaxValue, inclusive." + Environment.NewLine + "Parameter name: " + ex.ParamName, ex.Message);
                Assert.AreEqual("value", ex.ParamName);
            }
        }

        [TestMethod]
        public void OperationTimeout_GreaterThanLowerLimit()
        {
            var operationTimeout = TimeSpan.FromMilliseconds(int.MaxValue).Add(TimeSpan.FromMilliseconds(1));
            var connectionInfo = new PasswordConnectionInfo("host", 22, "admin", "pwd");
            var target = new NetConfClient(connectionInfo);

            try
            {
                target.OperationTimeout = operationTimeout;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("The timeout must represent a value between -1 and Int32.MaxValue, inclusive." + Environment.NewLine + "Parameter name: " + ex.ParamName, ex.Message);
                Assert.AreEqual("value", ex.ParamName);
            }
        }

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