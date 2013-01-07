using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for ChannelOpenMessageTest and is intended
    ///to contain all ChannelOpenMessageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ChannelOpenMessageTest : TestBase
    {
        /// <summary>
        ///A test for ChannelOpenMessage Constructor
        ///</summary>
        [TestMethod()]
        public void ChannelOpenMessageConstructorTest()
        {
            ChannelOpenMessage target = new ChannelOpenMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChannelOpenMessage Constructor
        ///</summary>
        [TestMethod()]
        public void ChannelOpenMessageConstructorTest1()
        {
            uint channelNumber = 0; // TODO: Initialize to an appropriate value
            uint initialWindowSize = 0; // TODO: Initialize to an appropriate value
            uint maximumPacketSize = 0; // TODO: Initialize to an appropriate value
            ChannelOpenInfo info = null; // TODO: Initialize to an appropriate value
            ChannelOpenMessage target = new ChannelOpenMessage(channelNumber, initialWindowSize, maximumPacketSize, info);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChannelType
        ///</summary>
        [TestMethod()]
        public void ChannelTypeTest()
        {
            ChannelOpenMessage target = new ChannelOpenMessage(); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ChannelType;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
