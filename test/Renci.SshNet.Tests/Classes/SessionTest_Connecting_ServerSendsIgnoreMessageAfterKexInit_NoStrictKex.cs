using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_ServerSendsIgnoreMessageAfterKexInit_NoStrictKex : SessionTest_ConnectingBase
    {
        protected override bool ServerSupportsStrictKex
        {
            get
            {
                return false;
            }
        }

        protected override void MITMAttackAfterKexInit()
        {
            var ignoreMessage = new IgnoreMessage();
            var ignore = ignoreMessage.GetPacket(8, null);

            // MitM sends ignore message to client
            _ = ServerSocket.Send(ignore, 4, ignore.Length - 4, SocketFlags.None);

            // MitM drops server message
            ServerOutboundPacketSequence++;
        }

        [TestMethod]
        public void DoesNotThrowException()
        {
            Session.Connect();
        }
    }
}
