using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for NetConfServerExceptionTest and is intended
    ///to contain all NetConfServerExceptionTest Unit Tests
    ///</summary>
    [TestClass]
    public class NetConfServerExceptionTest : TestBase
    {
        /// <summary>
        ///A test for NetConfServerException Constructor
        ///</summary>
        [TestMethod]
        public void NetConfServerExceptionConstructorTest()
        {
            NetConfServerException target = new NetConfServerException();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for NetConfServerException Constructor
        ///</summary>
        [TestMethod]
        public void NetConfServerExceptionConstructorTest1()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            NetConfServerException target = new NetConfServerException(message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for NetConfServerException Constructor
        ///</summary>
        [TestMethod]
        public void NetConfServerExceptionConstructorTest2()
        {
            string message = string.Empty; // TODO: Initialize to an appropriate value
            Exception innerException = null; // TODO: Initialize to an appropriate value
            NetConfServerException target = new NetConfServerException(message, innerException);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
