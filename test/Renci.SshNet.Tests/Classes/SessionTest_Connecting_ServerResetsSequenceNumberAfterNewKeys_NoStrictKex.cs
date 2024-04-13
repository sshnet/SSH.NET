using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_ServerResetsSequenceNumberAfterNewKeys_NoStrictKex : SessionTest_ConnectingBase
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
                return true;
            }
        }


        [TestMethod]
        public void ShouldNotThrowException()
        {
            Session.Connect();
        }
    }
}
