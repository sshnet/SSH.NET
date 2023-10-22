using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    /// Provides data for <see cref="SshNet.Channels.Channel.RequestReceived"/> event.
    /// </summary>
    [TestClass]
    public class ChannelRequestEventArgsTest : TestBase
    {
        [TestMethod]
        public void Complete()
        {
            var info = new ExitStatusRequestInfo(3u);

            var e = new ChannelRequestEventArgs(info);

            Assert.AreSame(info, e.Info);
        }

        [TestMethod]
        public void InfoIsNull()
        {
            const RequestInfo info = null;

            try
            {
                _ = new ChannelRequestEventArgs(info);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(typeof(ArgumentNullException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("info", ex.ParamName);
            }
        }
    }
}
