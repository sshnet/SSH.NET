using System.Globalization;
using System.Net.Sockets;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_ServerSendsDebugMessageAfterKexInit_ServerDoesNotSupportStrictKex : SessionTest_ConnectingBase
    {
        protected override void MITMAttack()
        {
            using var stream = new SshDataStream(0);
            stream.WriteByte(1);
            stream.Write("This is a debug message", Encoding.UTF8);
            stream.Write(CultureInfo.CurrentCulture.Name, Encoding.UTF8);

            var debugMessage = new DebugMessage();
            debugMessage.Load(stream.ToArray());

            var debug = debugMessage.GetPacket(8, null);
            _ = ServerSocket.Send(debug, 4, debug.Length - 4, SocketFlags.None);
        }

        [TestMethod]
        public void ThrowsSshConnectionException()
        {
            // Should we allow debug message during kex in non-strict-kex mode?
            // Probably better to keep this behavior as is, unless someone strongly disagree.
            Assert.ThrowsException<SshException>(Session.Connect);
        }
    }
}
