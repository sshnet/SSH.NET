using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_ServerSendsDisconnectMessageAfterKexInit_StrictKex : SessionTest_ConnectingBase
    {
        protected override bool ServerSupportsStrictKex
        {
            get
            {
                return true;
            }
        }

        protected override void ActionAfterKexInit()
        {
            var disconnectMessage = new DisconnectMessage(DisconnectReason.TooManyConnections, "too many connections");
            var disconnect = disconnectMessage.GetPacket(8, null);

            // Server sends disconnect message to client
            _ = ServerSocket.Send(disconnect, 4, disconnect.Length - 4, SocketFlags.None);

            ServerOutboundPacketSequence++;
        }

        [TestMethod]
        public void DisconnectIsAllowedDuringStrictKex()
        {
            var exception = Assert.ThrowsExactly<SshConnectionException>(Session.Connect);
            Assert.AreEqual(DisconnectReason.TooManyConnections, exception.DisconnectReason);
        }
    }
}
