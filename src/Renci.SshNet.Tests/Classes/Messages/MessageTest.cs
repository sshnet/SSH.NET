using Renci.SshNet.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages
{
    /// <summary>
    ///This is a test class for MessageTest and is intended
    ///to contain all MessageTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] // placeholders only
    public class MessageTest : TestBase
    {
        internal virtual Message CreateMessage()
        {
            // TODO: Instantiate an appropriate concrete class.
            Message target = null;
            return target;
        }

        /// <summary>
        ///A test for GetBytes
        ///</summary>
        [TestMethod]
        public void GetBytesTest()
        {
            Message target = CreateMessage(); // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            var actual = target.GetBytes();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod]
        public void ToStringTest()
        {
            Message target = CreateMessage(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            var actual = target.ToString();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
