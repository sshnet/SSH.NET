using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_ServerSendsMaxIgnoreMessagesBeforeKexInit : SessionTest_ConnectingBase
    {
        protected override void ActionBeforeKexInit()
        {
            var ignoreMessage = new IgnoreMessage();
            var ignore = ignoreMessage.GetPacket(8, null);
            for (uint i = 0; i < uint.MaxValue; i++)
            {
                // MitM sends ignore message to client
                _ = ServerSocket.Send(ignore, 4, ignore.Length - 4, SocketFlags.None);
            }
        }

        [TestMethod]
        [Ignore("It takes hours to send 4294967295 ignore messages.")]
        public void ShouldThrowSshConnectionException()
        {
            var exception = Assert.ThrowsException<SshConnectionException>(Session.Connect);
            Assert.AreEqual(DisconnectReason.KeyExchangeFailed, exception.DisconnectReason);
            Assert.AreEqual("Inbound packet sequence number is about to wrap during initial key exchange.", exception.Message);
        }
    }
}
