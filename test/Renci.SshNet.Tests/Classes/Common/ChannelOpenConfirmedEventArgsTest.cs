using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    /// Provides data for <see cref="SshNet.Channels.ClientChannel.OpenConfirmed"/> event.
    /// </summary>
    [TestClass]
    public class ChannelOpenConfirmedEventArgsTest : TestBase
    {
        [TestMethod]
        public void Complete()
        {
            const uint remoteChannelNumber = 12u;
            const uint initialWindowSize = 5u;
            const uint maximumPacketSize = 24555u;

            var e = new ChannelOpenConfirmedEventArgs(remoteChannelNumber, initialWindowSize, maximumPacketSize);

            Assert.AreEqual(remoteChannelNumber, e.ChannelNumber);
            Assert.AreEqual(initialWindowSize, e.InitialWindowSize);
            Assert.AreEqual(maximumPacketSize, e.MaximumPacketSize);
        }

        [TestMethod]
        public void Minimal()
        {
            const uint remoteChannelNumber = 0u;
            const uint initialWindowSize = 0u;
            const uint maximumPacketSize = 0u;

            var e = new ChannelOpenConfirmedEventArgs(remoteChannelNumber, initialWindowSize, maximumPacketSize);

            Assert.AreEqual(remoteChannelNumber, e.ChannelNumber);
            Assert.AreEqual(initialWindowSize, e.InitialWindowSize);
            Assert.AreEqual(maximumPacketSize, e.MaximumPacketSize);
        }
    }
}
