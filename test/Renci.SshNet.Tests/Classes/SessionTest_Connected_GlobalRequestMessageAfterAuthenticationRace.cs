using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Test for https://github.com/sshnet/SSH.NET/issues/8.
    /// </summary>
    [TestClass]
    public class SessionTest_Connected_GlobalRequestMessageAfterAuthenticationRace : SessionTest_ConnectedBase
    {
        private GlobalRequestMessage _globalRequestMessage;

        protected override void SetupData()
        {
            base.SetupData();

            _globalRequestMessage = new GlobalRequestMessage(Encoding.ASCII.GetBytes("ping-mocana-com"), false);
        }

        protected override void Act()
        {
        }

        protected override void ClientAuthentication_Callback()
        {
            var globalRequest = _globalRequestMessage.GetPacket(8, null);
            ServerSocket.Send(globalRequest, 4, globalRequest.Length - 4, SocketFlags.None);
        }

        [TestMethod]
        public void ErrorOccurredShouldNotBeRaised()
        {
            Assert.AreEqual(0, ErrorOccurredRegister.Count, ErrorOccurredRegister.AsString());
        }
    }
}
