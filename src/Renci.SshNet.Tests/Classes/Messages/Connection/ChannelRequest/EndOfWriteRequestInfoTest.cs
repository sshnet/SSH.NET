﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for EndOfWriteRequestInfoTest and is intended
    ///to contain all EndOfWriteRequestInfoTest Unit Tests
    ///</summary>
    [TestClass]
    public class EndOfWriteRequestInfoTest : TestBase
    {
        /// <summary>
        ///A test for EndOfWriteRequestInfo Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void EndOfWriteRequestInfoConstructorTest()
        {
            var target = new EndOfWriteRequestInfo();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for RequestName
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void RequestNameTest()
        {
            var target = new EndOfWriteRequestInfo(); // TODO: Initialize to an appropriate value
            
            var actual = target.RequestName;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
