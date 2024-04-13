using System.Globalization;
using System.Net.Sockets;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_ServerSendsDebugMessageAfterKexInit_NoStrictKex : SessionTest_ConnectingBase
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
            using var stream = new SshDataStream(0);
            stream.WriteByte(1);
            stream.Write("This is a debug message", Encoding.UTF8);
            stream.Write(CultureInfo.CurrentCulture.Name, Encoding.UTF8);

            var debugMessage = new DebugMessage();
            debugMessage.Load(stream.ToArray());
            var debug = debugMessage.GetPacket(8, null);

            // MitM sends debug message to client
            _ = ServerSocket.Send(debug, 4, debug.Length - 4, SocketFlags.None);

            // MitM drops server message
            ServerOutboundPacketSequence++;
        }

        [TestMethod]
        public void ThrowsSshException()
        {
            // Should we allow debug message during kex in non-strict-kex mode?
            // Probably better to keep this behavior as is, unless someone strongly disagree.
            var message = Assert.ThrowsException<SshException>(Session.Connect).Message;
            Assert.AreEqual("Message type 4 is not valid in the current context.", message);
        }
    }
}
