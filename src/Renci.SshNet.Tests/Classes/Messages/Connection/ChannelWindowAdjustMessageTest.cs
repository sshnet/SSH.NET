using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for ChannelWindowAdjustMessageTest and is intended
    ///to contain all ChannelWindowAdjustMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class ChannelWindowAdjustMessageTest : TestBase
    {
        /// <summary>
        ///A test for ChannelWindowAdjustMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelWindowAdjustMessageConstructorTest()
        {
            ChannelWindowAdjustMessage target = new ChannelWindowAdjustMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChannelWindowAdjustMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelWindowAdjustMessageConstructorTest1()
        {
            uint localChannelNumber = 0; // TODO: Initialize to an appropriate value
            uint bytesToAdd = 0; // TODO: Initialize to an appropriate value
            ChannelWindowAdjustMessage target = new ChannelWindowAdjustMessage(localChannelNumber, bytesToAdd);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
