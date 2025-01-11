using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_ServerSendsIgnoreMessageAfterKexInit_StrictKex : SessionTest_ConnectingBase
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
            var ignoreMessage = new IgnoreMessage();
            var ignore = ignoreMessage.GetPacket(8, null);

            // MitM sends ignore message to client
            _ = ServerSocket.Send(ignore, 4, ignore.Length - 4, SocketFlags.None);

            // MitM drops server message
            ServerOutboundPacketSequence++;
        }

        [TestMethod]
        public void ShouldThrowSshException()
        {
            var message = Assert.ThrowsException<SshException>(Session.Connect).Message;
            Assert.AreEqual("Message type 2 is not valid in the current context.", message);
        }
    }
}
