using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for ChannelEofMessageTest and is intended
    ///to contain all ChannelEofMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class ChannelEofMessageTest : TestBase
    {
        /// <summary>
        ///A test for ChannelEofMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelEofMessageConstructorTest()
        {
            ChannelEofMessage target = new ChannelEofMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChannelEofMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelEofMessageConstructorTest1()
        {
            uint localChannelNumber = 0; // TODO: Initialize to an appropriate value
            ChannelEofMessage target = new ChannelEofMessage(localChannelNumber);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
