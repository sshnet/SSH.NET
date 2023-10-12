using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for SshPassPhraseNullOrEmptyExceptionTest and is intended
    ///to contain all SshPassPhraseNullOrEmptyExceptionTest Unit Tests
    ///</summary>
    [TestClass]
    public class SshPassPhraseNullOrEmptyExceptionTest : TestBase
    {
        /// <summary>
        ///A test for SshPassPhraseNullOrEmptyException Constructor
        ///</summary>
        [TestMethod]
        public void SshPassPhraseNullOrEmptyExceptionConstructorTest()
        {
            SshPassPhraseNullOrEmptyException target = new SshPassPhraseNullOrEmptyException();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshPassPhraseNullOrEmptyException Constructor
        ///</summary>
        [TestMethod]
        public void SshPassPhraseNullOrEmptyExceptionConstructorTest1()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            SshPassPhraseNullOrEmptyException target = new SshPassPhraseNullOrEmptyException(message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshPassPhraseNullOrEmptyException Constructor
        ///</summary>
        [TestMethod]
        public void SshPassPhraseNullOrEmptyExceptionConstructorTest2()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            Exception innerException = null; // TODO: Initialize to an appropriate value
            SshPassPhraseNullOrEmptyException target = new SshPassPhraseNullOrEmptyException(message, innerException);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
