using Renci.SshNet.Messages.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Messages;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Messages.Authentication
{
    /// <summary>
    ///This is a test class for RequestMessageTest and is intended
    ///to contain all RequestMessageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RequestMessageTest : TestBase
    {
        /// <summary>
        ///A test for RequestMessage Constructor
        ///</summary>
        [TestMethod()]
        public void RequestMessageConstructorTest()
        {
            ServiceName serviceName = new ServiceName(); // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            RequestMessage target = new RequestMessage(serviceName, username);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for MethodName
        ///</summary>
        [TestMethod()]
        public void MethodNameTest()
        {
            ServiceName serviceName = new ServiceName(); // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            RequestMessage target = new RequestMessage(serviceName, username); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.MethodName;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
