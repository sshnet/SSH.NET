using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for RequestSuccessMessageTest and is intended
    ///to contain all RequestSuccessMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class RequestSuccessMessageTest : TestBase
    {
        /// <summary>
        ///A test for RequestSuccessMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void RequestSuccessMessageConstructorTest()
        {
            RequestSuccessMessage target = new RequestSuccessMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for RequestSuccessMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void RequestSuccessMessageConstructorTest1()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            RequestSuccessMessage target = new RequestSuccessMessage(boundPort);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
