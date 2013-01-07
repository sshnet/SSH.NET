using Renci.SshNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for ForwardedPortTest and is intended
    ///to contain all ForwardedPortTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ForwardedPortTest : TestBase
    {
        internal virtual ForwardedPort CreateForwardedPort()
        {
            // TODO: Instantiate an appropriate concrete class.
            ForwardedPort target = null;
            return target;
        }

        /// <summary>
        ///A test for Stop
        ///</summary>
        [TestMethod()]
        public void StopTest()
        {
            ForwardedPort target = CreateForwardedPort(); // TODO: Initialize to an appropriate value
            target.Stop();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Start
        ///</summary>
        [TestMethod()]
        public void StartTest()
        {
            ForwardedPort target = CreateForwardedPort(); // TODO: Initialize to an appropriate value
            target.Start();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
