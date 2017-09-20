using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for SftpPathNotFoundExceptionTest and is intended
    ///to contain all SftpPathNotFoundExceptionTest Unit Tests
    ///</summary>
    [TestClass]
    public class SftpPathNotFoundExceptionTest : TestBase
    {
        /// <summary>
        ///A test for SftpPathNotFoundException Constructor
        ///</summary>
        [TestMethod]
        public void SftpPathNotFoundExceptionConstructorTest()
        {
            SftpPathNotFoundException target = new SftpPathNotFoundException();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SftpPathNotFoundException Constructor
        ///</summary>
        [TestMethod]
        public void SftpPathNotFoundExceptionConstructorTest1()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            SftpPathNotFoundException target = new SftpPathNotFoundException(message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SftpPathNotFoundException Constructor
        ///</summary>
        [TestMethod]
        public void SftpPathNotFoundExceptionConstructorTest2()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            Exception innerException = null; // TODO: Initialize to an appropriate value
            SftpPathNotFoundException target = new SftpPathNotFoundException(message, innerException);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
