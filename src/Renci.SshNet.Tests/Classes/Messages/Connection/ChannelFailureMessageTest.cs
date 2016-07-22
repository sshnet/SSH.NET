using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for ChannelFailureMessageTest and is intended
    ///to contain all ChannelFailureMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class ChannelFailureMessageTest : TestBase
    {
        /// <summary>
        ///A test for ChannelFailureMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelFailureMessageConstructorTest()
        {
            ChannelFailureMessage target = new ChannelFailureMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChannelFailureMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelFailureMessageConstructorTest1()
        {
            uint localChannelNumber = 0; // TODO: Initialize to an appropriate value
            ChannelFailureMessage target = new ChannelFailureMessage(localChannelNumber);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
