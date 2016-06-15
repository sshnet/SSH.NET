using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Test for https://github.com/sshnet/SSH.NET/issues/8.
    /// </summary>
    [TestClass]
    public class SessionTest_Connected_GlobalRequestMessageAfterAuthenticationRace : SessionTest_ConnectedBase
    {
        protected override void Act()
        {
        }

        protected override void ClientAuthentication_Callback()
        {
            var globalRequestMessage = new GlobalRequestMessage(GlobalRequestName.TcpIpForward, false, "address", 70);
            var globalRequest = globalRequestMessage.GetPacket(8, null);
            ServerSocket.Send(globalRequest, 4, globalRequest.Length - 4, SocketFlags.None);
        }

        [TestMethod]
        public void ErrorOccurredShouldNotBeRaised()
        {
            Assert.AreEqual(0, ErrorOccurredRegister.Count);
        }
    }
}
