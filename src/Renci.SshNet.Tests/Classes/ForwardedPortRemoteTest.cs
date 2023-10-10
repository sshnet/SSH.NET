using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for remote port forwarding
    /// </summary>
    [TestClass]
    public partial class ForwardedPortRemoteTest : TestBase
    {
        /// <summary>
        ///A test for Stop
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void StopTest()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            var host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            var target = new ForwardedPortRemote(boundPort, host, port); // TODO: Initialize to an appropriate value
            target.Stop();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        [TestMethod]
        public void Start_NotAddedToClient()
        {
            const int boundPort = 80;
            var host = string.Empty;
            const uint port = 22;
            var target = new ForwardedPortRemote(boundPort, host, port);

            try
            {
                target.Start();
                Assert.Fail();
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Forwarded port is not added to a client.", ex.Message);
            }
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void DisposeTest()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            var host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            var target = new ForwardedPortRemote(boundPort, host, port); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ForwardedPortRemote Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ForwardedPortRemoteConstructorTest()
        {
            var boundHost = string.Empty; // TODO: Initialize to an appropriate value
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            var host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            var target = new ForwardedPortRemote(boundHost, boundPort, host, port);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ForwardedPortRemote Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ForwardedPortRemoteConstructorTest1()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            var host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            var target = new ForwardedPortRemote(boundPort, host, port);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
