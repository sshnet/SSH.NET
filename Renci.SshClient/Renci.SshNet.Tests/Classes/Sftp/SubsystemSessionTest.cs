using Renci.SshNet.Sftp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for SubsystemSessionTest and is intended
    ///to contain all SubsystemSessionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SubsystemSessionTest : TestBase
    {
        internal virtual SubsystemSession CreateSubsystemSession()
        {
            // TODO: Instantiate an appropriate concrete class.
            SubsystemSession target = null;
            return target;
        }

        /// <summary>
        ///A test for Connect
        ///</summary>
        [TestMethod()]
        public void ConnectTest()
        {
            SubsystemSession target = CreateSubsystemSession(); // TODO: Initialize to an appropriate value
            target.Connect();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Disconnect
        ///</summary>
        [TestMethod()]
        public void DisconnectTest()
        {
            SubsystemSession target = CreateSubsystemSession(); // TODO: Initialize to an appropriate value
            target.Disconnect();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            SubsystemSession target = CreateSubsystemSession(); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendData
        ///</summary>
        [TestMethod()]
        public void SendDataTest()
        {
            SubsystemSession target = CreateSubsystemSession(); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            target.SendData(data);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
