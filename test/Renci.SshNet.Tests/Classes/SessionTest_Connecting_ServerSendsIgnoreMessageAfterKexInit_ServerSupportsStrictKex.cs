using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_ServerSendsIgnoreMessageAfterKexInit_ServerSupportsStrictKex : SessionTest_ConnectingBase
    {
        protected override bool IsStrictKex
        {
            get
            {
                return true;
            }
        }

        protected override void MITMAttack()
        {
            var ignoreMessage = new IgnoreMessage();
            var ignore = ignoreMessage.GetPacket(8, null);
            _ = ServerSocket.Send(ignore, 4, ignore.Length - 4, SocketFlags.None);
        }

        [TestMethod]
        public void ShouldThrowSshConnectionException()
        {
            Assert.ThrowsException<SshException>(Session.Connect);
        }
    }
}
