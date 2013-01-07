using Renci.SshNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for BaseClientTest and is intended
    ///to contain all BaseClientTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BaseClientTest : TestBase
    {
        internal virtual BaseClient CreateBaseClient()
        {
            // TODO: Instantiate an appropriate concrete class.
            BaseClient target = null;
            return target;
        }

        /// <summary>
        ///A test for KeepAliveInterval
        ///</summary>
        [TestMethod()]
        public void KeepAliveIntervalTest()
        {
            BaseClient target = CreateBaseClient(); // TODO: Initialize to an appropriate value
            TimeSpan expected = new TimeSpan(); // TODO: Initialize to an appropriate value
            TimeSpan actual;
            target.KeepAliveInterval = expected;
            actual = target.KeepAliveInterval;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsConnected
        ///</summary>
        [TestMethod()]
        public void IsConnectedTest()
        {
            BaseClient target = CreateBaseClient(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsConnected;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SendKeepAlive
        ///</summary>
        [TestMethod()]
        public void SendKeepAliveTest()
        {
            BaseClient target = CreateBaseClient(); // TODO: Initialize to an appropriate value
            target.SendKeepAlive();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            BaseClient target = CreateBaseClient(); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Disconnect
        ///</summary>
        [TestMethod()]
        public void DisconnectTest()
        {
            BaseClient target = CreateBaseClient(); // TODO: Initialize to an appropriate value
            target.Disconnect();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Connect
        ///</summary>
        [TestMethod()]
        public void ConnectTest()
        {
            BaseClient target = CreateBaseClient(); // TODO: Initialize to an appropriate value
            target.Connect();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
