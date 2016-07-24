using Renci.SshNet.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages
{
    /// <summary>
    ///This is a test class for MessageAttributeTest and is intended
    ///to contain all MessageAttributeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MessageAttributeTest : TestBase
    {
        /// <summary>
        ///A test for MessageAttribute Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void MessageAttributeConstructorTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            byte number = 0; // TODO: Initialize to an appropriate value
            MessageAttribute target = new MessageAttribute(name, number);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void NameTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            byte number = 0; // TODO: Initialize to an appropriate value
            MessageAttribute target = new MessageAttribute(name, number); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            target.Name = expected;
            var actual = target.Name;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Number
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void NumberTest()
        {
            string name = string.Empty; // TODO: Initialize to an appropriate value
            byte number = 0; // TODO: Initialize to an appropriate value
            MessageAttribute target = new MessageAttribute(name, number); // TODO: Initialize to an appropriate value
            byte expected = 0; // TODO: Initialize to an appropriate value
            target.Number = expected;
            var actual = target.Number;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
