﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for SshExceptionTest and is intended
    ///to contain all SshExceptionTest Unit Tests
    ///</summary>
    [TestClass]
    public class SshExceptionTest : TestBase
    {
        /// <summary>
        ///A test for SshException Constructor
        ///</summary>
        [TestMethod]
        public void SshExceptionConstructorTest()
        {
            SshException target = new SshException();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshException Constructor
        ///</summary>
        [TestMethod]
        public void SshExceptionConstructorTest1()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            SshException target = new SshException(message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshException Constructor
        ///</summary>
        [TestMethod]
        public void SshExceptionConstructorTest2()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            Exception inner = null; // TODO: Initialize to an appropriate value
            SshException target = new SshException(message, inner);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
