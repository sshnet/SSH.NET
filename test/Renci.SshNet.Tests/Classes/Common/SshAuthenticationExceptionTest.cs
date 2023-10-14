using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for SshAuthenticationExceptionTest and is intended
    ///to contain all SshAuthenticationExceptionTest Unit Tests
    ///</summary>
    [TestClass]
    public class SshAuthenticationExceptionTest
    {
        /// <summary>
        ///A test for SshAuthenticationException Constructor
        ///</summary>
        [TestMethod]
        public void SshAuthenticationExceptionConstructorTest()
        {
            SshAuthenticationException target = new SshAuthenticationException();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshAuthenticationException Constructor
        ///</summary>
        [TestMethod]
        public void SshAuthenticationExceptionConstructorTest1()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            SshAuthenticationException target = new SshAuthenticationException(message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshAuthenticationException Constructor
        ///</summary>
        [TestMethod]
        public void SshAuthenticationExceptionConstructorTest2()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            Exception innerException = null; // TODO: Initialize to an appropriate value
            SshAuthenticationException target = new SshAuthenticationException(message, innerException);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
