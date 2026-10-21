using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// The server starts a key re-exchange (SSH_MSG_KEXINIT) and then, before it sends its SSH_MSG_NEWKEYS,
    /// sends an SSH_MSG_CHANNEL_DATA. This is what the client sees when the server has an SFTP response
    /// to a request that crossed the KEXINIT on the wire (RFC 4253 section 7.1 asks a sender not to do that,
    /// but not every server is that strict, and the data is still protected by the old keys at this point).
    /// </summary>
    /// <remarks>
    /// The client must neither fail the session ("Message type 94 is not valid in the current context")
    /// nor dispatch the channel data while the key exchange is still in progress: a handler that answers
    /// with an SSH_MSG_CHANNEL_WINDOW_ADJUST (as <c>Channel.OnData</c> does) would otherwise block on the
    /// key exchange gate, which can only be opened by the very thread that is running the handler.
    /// </remarks>
    [TestClass]
    public class SessionTest_Connected_ServerRekeysAndSendsChannelDataBeforeNewKeys : SessionTest_ConnectedBase
    {
        private const uint LocalChannelNumber = 0;

        private byte[] _channelData;
        private List<byte[]> _channelDataReceived;
        private ManualResetEventSlim _windowAdjustReceivedByServer;

        protected override void SetupData()
        {
            base.SetupData();

            _channelData = new byte[] { 1, 2, 3, 4, 5 };
            _channelDataReceived = new List<byte[]>();
            _windowAdjustReceivedByServer = new ManualResetEventSlim();

            Session.ChannelDataReceived += (sender, args) =>
                {
                    lock (_channelDataReceived)
                    {
                        // the message may reference a larger buffer, only Offset and Size delimit the data
                        _channelDataReceived.Add(args.Message.Data.Skip(args.Message.Offset).Take(args.Message.Size).ToArray());
                    }

                    // Answer the way Channel.OnData does when the receive window needs to be adjusted.
                    Session.SendMessage(new ChannelWindowAdjustMessage(LocalChannelNumber, 1000));
                };

            ServerListener.BytesReceived += (received, socket) =>
                {
                    if (GetMessageNumbers(received).Contains((byte)93))
                    {
                        _windowAdjustReceivedByServer.Set();
                    }
                };
        }

        protected override void Act()
        {
            var keyExchangeInit = new KeyExchangeInitMessage
            {
                CompressionAlgorithmsClientToServer = new string[0],
                CompressionAlgorithmsServerToClient = new string[0],
                EncryptionAlgorithmsClientToServer = new string[0],
                EncryptionAlgorithmsServerToClient = new string[0],
                KeyExchangeAlgorithms = new[] { KeyExchangeAlgorithm },
                LanguagesClientToServer = new string[0],
                LanguagesServerToClient = new string[0],
                MacAlgorithmsClientToServer = new string[0],
                MacAlgorithmsServerToClient = new string[0],
                ServerHostKeyAlgorithms = new string[0]
            }.GetPacket(8, null);

            var channelData = new ChannelDataMessage(LocalChannelNumber, _channelData).GetPacket(8, null);

            // The receive buffer of the session is 4 KB. These packets that follow the channel data (SSH_MSG_IGNORE is
            // allowed during a key exchange) fill it up, so that the session has to reuse the part of the buffer that
            // holds the channel data while that is still waiting for the key exchange to complete.
            var filler = new List<byte[]>();
            for (var i = 0; i < 4; i++)
            {
                filler.Add(new IgnoreMessage(Enumerable.Repeat((byte)0xEE, 1500).ToArray()).GetPacket(8, null));
            }

            // Send all packets at once, so that the channel data is guaranteed to follow the KEXINIT on the wire. The
            // server's SSH_MSG_NEWKEYS is sent by the KeyExchangeInitReceived handler in the base class, and therefore
            // arrives after the channel data.
            var packets = Concatenate(new[] { keyExchangeInit, channelData }.Concat(filler));

            _ = ServerSocket.Send(packets, 0, packets.Length, SocketFlags.None);

            _ = _windowAdjustReceivedByServer.Wait(TimeSpan.FromSeconds(5));
        }

        [TestMethod]
        public void ErrorOccurredIsNeverRaised()
        {
            Assert.IsEmpty(ErrorOccurredRegister, ErrorOccurredRegister.AsString());
        }

        [TestMethod]
        public void IsConnectedShouldReturnTrue()
        {
            Assert.IsTrue(Session.IsConnected);
        }

        [TestMethod]
        public void ChannelDataIsDeliveredExactlyOnce()
        {
            lock (_channelDataReceived)
            {
                Assert.HasCount(1, _channelDataReceived);
                CollectionAssert.AreEqual(_channelData, _channelDataReceived[0]);
            }
        }

        [TestMethod]
        public void WindowAdjustSentFromChannelDataHandlerReachesServer()
        {
            Assert.IsTrue(_windowAdjustReceivedByServer.IsSet);
        }

        /// <summary>
        /// Concatenates the packets, without the 4 bytes at the start of each of them that
        /// are reserved for the packet sequence number (which is not sent over the wire).
        /// </summary>
        private static byte[] Concatenate(IEnumerable<byte[]> packets)
        {
            return packets.SelectMany(p => p.Skip(4)).ToArray();
        }

        /// <summary>
        /// Returns the message numbers of the (unencrypted) packets in <paramref name="received"/>.
        /// </summary>
        private static IEnumerable<byte> GetMessageNumbers(byte[] received)
        {
            var offset = 0;
            while (offset + 6 <= received.Length)
            {
                var packetLength = (received[offset] << 24) | (received[offset + 1] << 16) | (received[offset + 2] << 8) | received[offset + 3];

                yield return received[offset + 5];

                offset += 4 + packetLength;
            }
        }
    }
}
