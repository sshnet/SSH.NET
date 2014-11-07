using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for dynamic port forwarding
    /// </summary>
    [TestClass]
    public partial class ForwardedPortDynamicTest : TestBase
    {
        /// <summary>
        ///A test for ForwardedPortDynamic Constructor
        ///</summary>
        [TestMethod()]
        public void ForwardedPortDynamicConstructorTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortDynamic target = new ForwardedPortDynamic(host, port);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ForwardedPortDynamic Constructor
        ///</summary>
        [TestMethod()]
        public void ForwardedPortDynamicConstructorTest1()
        {
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortDynamic target = new ForwardedPortDynamic(port);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}