using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortRemoteTest_Stop_PortStarted_ChannelBound : ForwardedPortRemoteTest_Dispose_PortStarted_ChannelBound
    {
        protected override void Act()
        {
            ForwardedPort.Stop();
        }
    }
}
