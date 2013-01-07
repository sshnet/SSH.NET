using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    [TestClass]
    public partial class SessionTest : TestBase
    {
        /// <summary>
        ///A test for SessionSemaphore
        ///</summary>
        [TestMethod()]
        public void SessionSemaphoreTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            SemaphoreLight actual;
            actual = target.SessionSemaphore;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsConnected
        ///</summary>
        [TestMethod()]
        public void IsConnectedTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsConnected;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ClientInitMessage
        ///</summary>
        [TestMethod()]
        public void ClientInitMessageTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            Message actual;
            actual = target.ClientInitMessage;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for UnRegisterMessage
        ///</summary>
        [TestMethod()]
        public void UnRegisterMessageTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            string messageName = string.Empty; // TODO: Initialize to an appropriate value
            target.UnRegisterMessage(messageName);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for RegisterMessage
        ///</summary>
        [TestMethod()]
        public void RegisterMessageTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            string messageName = string.Empty; // TODO: Initialize to an appropriate value
            target.RegisterMessage(messageName);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Disconnect
        ///</summary>
        [TestMethod()]
        public void DisconnectTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            target.Disconnect();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Connect
        ///</summary>
        [TestMethod()]
        public void ConnectTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            target.Connect();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

    }
}