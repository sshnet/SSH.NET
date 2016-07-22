using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for ChannelCloseMessageTest and is intended
    ///to contain all ChannelCloseMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class ChannelCloseMessageTest : TestBase
    {
        /// <summary>
        ///A test for ChannelCloseMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelCloseMessageConstructorTest()
        {
            ChannelCloseMessage target = new ChannelCloseMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChannelCloseMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelCloseMessageConstructorTest1()
        {
            uint localChannelNumber = 0; // TODO: Initialize to an appropriate value
            ChannelCloseMessage target = new ChannelCloseMessage(localChannelNumber);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
