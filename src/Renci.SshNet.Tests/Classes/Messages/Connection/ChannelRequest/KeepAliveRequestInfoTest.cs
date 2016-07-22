using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for KeepAliveRequestInfoTest and is intended
    ///to contain all KeepAliveRequestInfoTest Unit Tests
    ///</summary>
    [TestClass]
    public class KeepAliveRequestInfoTest : TestBase
    {
        /// <summary>
        ///A test for KeepAliveRequestInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void KeepAliveRequestInfoConstructorTest()
        {
            KeepAliveRequestInfo target = new KeepAliveRequestInfo();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for RequestName
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void RequestNameTest()
        {
            KeepAliveRequestInfo target = new KeepAliveRequestInfo(); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.RequestName;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
