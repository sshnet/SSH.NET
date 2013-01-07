using Renci.SshNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for CommandAsyncResultTest and is intended
    ///to contain all CommandAsyncResultTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommandAsyncResultTest : TestBase
    {
        /// <summary>
        ///A test for BytesSent
        ///</summary>
        [TestMethod()]
        public void BytesSentTest()
        {
            SshCommand command = null; // TODO: Initialize to an appropriate value
            CommandAsyncResult target = new CommandAsyncResult(command); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.BytesSent = expected;
            actual = target.BytesSent;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for BytesReceived
        ///</summary>
        [TestMethod()]
        public void BytesReceivedTest()
        {
            SshCommand command = null; // TODO: Initialize to an appropriate value
            CommandAsyncResult target = new CommandAsyncResult(command); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.BytesReceived = expected;
            actual = target.BytesReceived;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
