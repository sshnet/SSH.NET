using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortDynamicTest : TestBase
    {
        [TestMethod()]
        public void Constructor_HostAndPort()
        {
            var host = new Random().Next().ToString(CultureInfo.InvariantCulture);
            var port = (uint) new Random().Next(0, int.MaxValue);

            var target = new ForwardedPortDynamic(host, port);

            Assert.AreSame(host, target.BoundHost);
            Assert.AreEqual(port, target.BoundPort);
        }

        [TestMethod()]
        public void Constructor_Port()
        {
            var port = (uint)new Random().Next(0, int.MaxValue);

            var target = new ForwardedPortDynamic(port);

            Assert.AreSame(string.Empty, target.BoundHost);
            Assert.AreEqual(port, target.BoundPort);
        }
    }
}