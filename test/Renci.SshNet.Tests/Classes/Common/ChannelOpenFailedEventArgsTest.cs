using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    /// Provides data for <see cref="SshNet.Channels.ClientChannel.OpenFailed"/> event.
    /// </summary>
    [TestClass]
    public class ChannelOpenFailedEventArgsTest : TestBase
    {
        [TestMethod]
        public void Complete()
        {
            const uint channelNumber = 12u;
            const uint reasonCode = 5u;
            const string description = "Warp failure";
            const string language = "EN";

            var e = new ChannelOpenFailedEventArgs(channelNumber, reasonCode, description, language);

            Assert.AreEqual(channelNumber, e.ChannelNumber);
            Assert.AreEqual(reasonCode, e.ReasonCode);
            Assert.AreSame(description, e.Description);
            Assert.AreSame(language, e.Language);
        }

        [TestMethod]
        public void Minimal()
        {
            const uint channelNumber = 0u;
            const uint reasonCode = 0u;
            const string description = null;
            const string language = null;

            var e = new ChannelOpenFailedEventArgs(channelNumber, reasonCode, description, language);

            Assert.AreEqual(channelNumber, e.ChannelNumber);
            Assert.AreEqual(reasonCode, e.ReasonCode);
            Assert.IsNull(e.Description);
            Assert.IsNull(e.Language);
        }
    }
}
