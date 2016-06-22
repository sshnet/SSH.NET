using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_CHANNEL_REQUEST message.
    /// </summary>
    [TestClass]
    [Ignore] // placeholders only
    public class ChannelRequestMessageTest : TestBase
    {
        /// <summary>
        ///A test for ChannelRequestMessage Constructor
        ///</summary>
        [TestMethod()]
        public void ChannelRequestMessageConstructorTest()
        {
            ChannelRequestMessage target = new ChannelRequestMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ChannelRequestMessage Constructor
        ///</summary>
        [TestMethod()]
        public void ChannelRequestMessageConstructorTest1()
        {
            uint localChannelName = 0; // TODO: Initialize to an appropriate value
            RequestInfo info = null; // TODO: Initialize to an appropriate value
            ChannelRequestMessage target = new ChannelRequestMessage(localChannelName, info);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}