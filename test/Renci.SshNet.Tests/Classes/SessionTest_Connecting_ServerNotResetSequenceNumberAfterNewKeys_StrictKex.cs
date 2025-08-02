using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_ServerNotResetSequenceNumberAfterNewKeys_StrictKex : SessionTest_ConnectingBase
    {
        protected override bool ServerSupportsStrictKex
        {
            get
            {
                return true;
            }
        }

        protected override bool ServerResetsSequenceAfterSendingNewKeys
        {
            get
            {
                return false;
            }
        }


        [TestMethod]
        public void ShouldThrowSshConnectionException()
        {
            var reason = Assert.ThrowsExactly<SshConnectionException>(Session.Connect).DisconnectReason;
            Assert.AreEqual(DisconnectReason.MacError, reason);
        }
    }
}
