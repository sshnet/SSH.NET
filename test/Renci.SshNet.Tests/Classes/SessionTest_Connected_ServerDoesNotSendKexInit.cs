using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    internal class SessionTest_Connected_ServerDoesNotSendKexInit : SessionTest_ConnectedBase
    {
        protected override void SetupData()
        {
            WaitForClientKeyExchangeInit = true;

            base.SetupData();
        }

        protected override void Act()
        {
        }

        [TestMethod]
        public void ConnectShouldSucceed()
        {
        }
    }
}
