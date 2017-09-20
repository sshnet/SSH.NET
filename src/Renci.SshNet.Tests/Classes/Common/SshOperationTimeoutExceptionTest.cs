using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for SshOperationTimeoutExceptionTest and is intended
    ///to contain all SshOperationTimeoutExceptionTest Unit Tests
    ///</summary>
    [TestClass]
    public class SshOperationTimeoutExceptionTest : TestBase
    {
        /// <summary>
        ///A test for SshOperationTimeoutException Constructor
        ///</summary>
        [TestMethod]
        public void SshOperationTimeoutExceptionConstructorTest()
        {
            SshOperationTimeoutException target = new SshOperationTimeoutException();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshOperationTimeoutException Constructor
        ///</summary>
        [TestMethod]
        public void SshOperationTimeoutExceptionConstructorTest1()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            SshOperationTimeoutException target = new SshOperationTimeoutException(message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshOperationTimeoutException Constructor
        ///</summary>
        [TestMethod]
        public void SshOperationTimeoutExceptionConstructorTest2()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            Exception innerException = null; // TODO: Initialize to an appropriate value
            SshOperationTimeoutException target = new SshOperationTimeoutException(message, innerException);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
