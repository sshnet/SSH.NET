using Renci.SshNet.Messages.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Transport
{
    /// <summary>
    ///This is a test class for IgnoreMessageTest and is intended
    ///to contain all IgnoreMessageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IgnoreMessageTest : TestBase
    {
        /// <summary>
        ///A test for IgnoreMessage Constructor
        ///</summary>
        [TestMethod()]
        public void IgnoreMessageConstructorTest()
        {
            IgnoreMessage target = new IgnoreMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for IgnoreMessage Constructor
        ///</summary>
        [TestMethod()]
        public void IgnoreMessageConstructorTest1()
        {
            byte[] data = null; // TODO: Initialize to an appropriate value
            IgnoreMessage target = new IgnoreMessage(data);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
