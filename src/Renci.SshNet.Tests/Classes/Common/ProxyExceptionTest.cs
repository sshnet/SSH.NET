using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for ProxyExceptionTest and is intended
    ///to contain all ProxyExceptionTest Unit Tests
    ///</summary>
    [TestClass]
    public class ProxyExceptionTest : TestBase
    {
        /// <summary>
        ///A test for ProxyException Constructor
        ///</summary>
        [TestMethod]
        public void ProxyExceptionConstructorTest()
        {
            ProxyException target = new ProxyException();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ProxyException Constructor
        ///</summary>
        [TestMethod]
        public void ProxyExceptionConstructorTest1()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            ProxyException target = new ProxyException(message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ProxyException Constructor
        ///</summary>
        [TestMethod]
        public void ProxyExceptionConstructorTest2()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            Exception innerException = null; // TODO: Initialize to an appropriate value
            ProxyException target = new ProxyException(message, innerException);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
