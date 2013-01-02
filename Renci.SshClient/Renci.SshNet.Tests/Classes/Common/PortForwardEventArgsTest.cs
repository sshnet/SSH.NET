using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using System;
using System.Net;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    /// Provides data for <see cref="Renci.SshNet.ForwardedPort.RequestReceived"/> event.
    /// </summary>
    [TestClass]
    public class PortForwardEventArgsTest : TestBase
    {
        [TestMethod]
        [Description("Test passing null to constructor of PortForwardEventArgs.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_PortForwardEventArgs_Host_Null()
        {
            var args = new PortForwardEventArgs(null, 80);
        }

        [TestMethod]
        [Description("Test passing an invalid port to constructor of PortForwardEventArgs.")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_PortForwardEventArgs_Port_Invalid()
        {
            var args = new PortForwardEventArgs("string", IPEndPoint.MaxPort + 1);
        }
    }
}