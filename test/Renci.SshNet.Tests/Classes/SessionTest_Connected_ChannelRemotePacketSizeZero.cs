using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connected_ChannelRemotePacketSizeZero : SessionTest_ConnectedBase
    {
        protected override void Act()
        {
        }

        [TestMethod]
        public void ChannelSendDataReturns()
        {
            var channel = ((ISession)Session).CreateChannelSession();

            ServerListener.BytesReceived += (received, socket) =>
            {
                if (received.Length > 5 && received[5] == 90)
                {
                    // This is the SSH_MSG_CHANNEL_OPEN.
                    // Send the confirmation with maximum_packet_size=0.

                    var confirmation = new ChannelOpenConfirmationMessage(
                        localChannelNumber: channel.LocalChannelNumber,
                        initialWindowSize: 2048,
                        maximumPacketSize: 0,
                        remoteChannelNumber: 1);

                    var packet = confirmation.GetPacket(8, null);
                    _ = socket.Send(packet, 4, packet.Length - 4, SocketFlags.None);
                }
            };

            channel.Open();

            channel.SendData("test"u8.ToArray()); // Regression test against infinite loop.
        }
    }
}
