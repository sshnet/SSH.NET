using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for ChannelDataMessageTest and is intended
    ///to contain all ChannelDataMessageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ChannelDataMessageTest : TestBase
    {
        /// <summary>
        ///A test for ChannelDataMessage Constructor
        ///</summary>
        [TestMethod()]
        public void ChannelDataMessageConstructorTest()
        {
            ChannelDataMessage target = new ChannelDataMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChannelDataMessage Constructor
        ///</summary>
        [TestMethod()]
        public void ChannelDataMessageConstructorTest1()
        {
            uint localChannelNumber = 0; // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            ChannelDataMessage target = new ChannelDataMessage(localChannelNumber, data);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
