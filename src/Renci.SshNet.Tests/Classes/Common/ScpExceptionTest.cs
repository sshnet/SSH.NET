using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{   
    /// <summary>
    ///This is a test class for ScpExceptionTest and is intended
    ///to contain all ScpExceptionTest Unit Tests
    ///</summary>
    [TestClass]
    public class ScpExceptionTest : TestBase
    {
        /// <summary>
        ///A test for ScpException Constructor
        ///</summary>
        [TestMethod]
        public void ScpExceptionConstructorTest()
        {
            ScpException target = new ScpException();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ScpException Constructor
        ///</summary>
        [TestMethod]
        public void ScpExceptionConstructorTest1()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            ScpException target = new ScpException(message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ScpException Constructor
        ///</summary>
        [TestMethod]
        public void ScpExceptionConstructorTest2()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            Exception innerException = null; // TODO: Initialize to an appropriate value
            ScpException target = new ScpException(message, innerException);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
