using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for SftpPermissionDeniedExceptionTest and is intended
    ///to contain all SftpPermissionDeniedExceptionTest Unit Tests
    ///</summary>
    [TestClass]
    public class SftpPermissionDeniedExceptionTest : TestBase
    {
        /// <summary>
        ///A test for SftpPermissionDeniedException Constructor
        ///</summary>
        [TestMethod]
        public void SftpPermissionDeniedExceptionConstructorTest()
        {
            SftpPermissionDeniedException target = new SftpPermissionDeniedException();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SftpPermissionDeniedException Constructor
        ///</summary>
        [TestMethod]
        public void SftpPermissionDeniedExceptionConstructorTest1()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            SftpPermissionDeniedException target = new SftpPermissionDeniedException(message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SftpPermissionDeniedException Constructor
        ///</summary>
        [TestMethod]
        public void SftpPermissionDeniedExceptionConstructorTest2()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            Exception innerException = null; // TODO: Initialize to an appropriate value
            SftpPermissionDeniedException target = new SftpPermissionDeniedException(message, innerException);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
