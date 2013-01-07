using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for GlobalRequestMessageTest and is intended
    ///to contain all GlobalRequestMessageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GlobalRequestMessageTest : TestBase
    {
        /// <summary>
        ///A test for GlobalRequestMessage Constructor
        ///</summary>
        [TestMethod()]
        public void GlobalRequestMessageConstructorTest()
        {
            GlobalRequestMessage target = new GlobalRequestMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for GlobalRequestMessage Constructor
        ///</summary>
        [TestMethod()]
        public void GlobalRequestMessageConstructorTest1()
        {
            GlobalRequestName requestName = new GlobalRequestName(); // TODO: Initialize to an appropriate value
            bool wantReply = false; // TODO: Initialize to an appropriate value
            GlobalRequestMessage target = new GlobalRequestMessage(requestName, wantReply);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for GlobalRequestMessage Constructor
        ///</summary>
        [TestMethod()]
        public void GlobalRequestMessageConstructorTest2()
        {
            GlobalRequestName requestName = new GlobalRequestName(); // TODO: Initialize to an appropriate value
            bool wantReply = false; // TODO: Initialize to an appropriate value
            string addressToBind = string.Empty; // TODO: Initialize to an appropriate value
            uint portToBind = 0; // TODO: Initialize to an appropriate value
            GlobalRequestMessage target = new GlobalRequestMessage(requestName, wantReply, addressToBind, portToBind);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
