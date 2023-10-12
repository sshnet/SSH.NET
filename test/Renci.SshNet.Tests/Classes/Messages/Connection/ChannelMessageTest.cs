using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for ChannelMessageTest and is intended
    ///to contain all ChannelMessageTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] // placeholders only
    public class ChannelMessageTest : TestBase
    {
        internal virtual ChannelMessage CreateChannelMessage()
        {
            // TODO: Instantiate an appropriate concrete class.
            ChannelMessage target = null;
            return target;
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            var target = CreateChannelMessage(); // TODO: Initialize to an appropriate value
            var expected = string.Empty; // TODO: Initialize to an appropriate value
            var actual = target.ToString();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
