using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    /// Base class forTest : TestBaseall channel related events.
    /// </summary>
    [TestClass]
    public class ChannelEventArgsTest : TestBase
    {
        [TestMethod]
        public void Complete()
        {
            var e = new ChannelEventArgs(12u);

            Assert.AreEqual(12U, e.ChannelNumber);
        }

        [TestMethod]
        public void Minimal()
        {
            var e = new ChannelEventArgs(0U);

            Assert.AreEqual(0U, e.ChannelNumber);
        }
    }
}
