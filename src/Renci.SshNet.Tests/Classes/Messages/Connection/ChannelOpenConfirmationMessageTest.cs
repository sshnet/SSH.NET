using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for ChannelOpenConfirmationMessageTest and is intended
    ///to contain all ChannelOpenConfirmationMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class ChannelOpenConfirmationMessageTest : TestBase
    {
        /// <summary>
        ///A test for ChannelOpenConfirmationMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelOpenConfirmationMessageConstructorTest()
        {
            ChannelOpenConfirmationMessage target = new ChannelOpenConfirmationMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChannelOpenConfirmationMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelOpenConfirmationMessageConstructorTest1()
        {
            uint localChannelNumber = 0; // TODO: Initialize to an appropriate value
            uint initialWindowSize = 0; // TODO: Initialize to an appropriate value
            uint maximumPacketSize = 0; // TODO: Initialize to an appropriate value
            uint remoteChannelNumber = 0; // TODO: Initialize to an appropriate value
            ChannelOpenConfirmationMessage target = new ChannelOpenConfirmationMessage(localChannelNumber, initialWindowSize, maximumPacketSize, remoteChannelNumber);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
