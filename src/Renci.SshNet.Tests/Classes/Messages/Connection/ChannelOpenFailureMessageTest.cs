using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for ChannelOpenFailureMessageTest and is intended
    ///to contain all ChannelOpenFailureMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class ChannelOpenFailureMessageTest : TestBase
    {
        /// <summary>
        ///A test for ChannelOpenFailureMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelOpenFailureMessageConstructorTest()
        {
            var target = new ChannelOpenFailureMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChannelOpenFailureMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ChannelOpenFailureMessageConstructorTest1()
        {
            uint localChannelNumber = 0; // TODO: Initialize to an appropriate value
            var description = string.Empty; // TODO: Initialize to an appropriate value
            uint reasonCode = 0; // TODO: Initialize to an appropriate value
            var target = new ChannelOpenFailureMessage(localChannelNumber, description, reasonCode);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
