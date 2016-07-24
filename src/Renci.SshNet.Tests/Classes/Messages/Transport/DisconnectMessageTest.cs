using Renci.SshNet.Messages.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Transport
{
    /// <summary>
    ///This is a test class for DisconnectMessageTest and is intended
    ///to contain all DisconnectMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class DisconnectMessageTest : TestBase
    {
        /// <summary>
        ///A test for DisconnectMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void DisconnectMessageConstructorTest()
        {
            DisconnectMessage target = new DisconnectMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for DisconnectMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void DisconnectMessageConstructorTest1()
        {
            DisconnectReason reasonCode = new DisconnectReason(); // TODO: Initialize to an appropriate value
            string message = string.Empty; // TODO: Initialize to an appropriate value
            DisconnectMessage target = new DisconnectMessage(reasonCode, message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
