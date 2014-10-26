using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public partial class ChannelDirectTcpipTestTest : TestBase
    {
        private Mock<ISession> _sessionMock;
        private Mock<IForwardedPort> _forwardedPortMock;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private string _remoteHost;
        private uint _port;
        private Socket _socket;
        private uint _localChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private uint _remoteChannelNumber;

        protected override void OnInit()
        {
            base.OnInit();

            var random = new Random();

            _localWindowSize = (uint) random.Next(0, int.MaxValue);
            _localPacketSize = (uint) random.Next(0, int.MaxValue);
            _remoteHost = random.Next().ToString(CultureInfo.InvariantCulture);
            _port = (uint) random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort);
            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint)random.Next(100, 200);
            _remoteChannelNumber = (uint)random.Next(0, int.MaxValue);

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _forwardedPortMock = new Mock<IForwardedPort>(MockBehavior.Strict);
        }

        [TestMethod]
        public void SocketShouldBeClosedAndBindShouldEndWhenForwardedPortSignalsClosingEvent()
        {
            _sessionMock.Setup(p => p.NextChannelNumber).Returns(_localChannelNumber);
            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.Setup(p => p.SendMessage(It.IsAny<ChannelOpenMessage>()))
                .Callback<Message>(m => _sessionMock.Raise(p => p.ChannelOpenConfirmationReceived += null,
                    new MessageEventArgs<ChannelOpenConfirmationMessage>(
                        new ChannelOpenConfirmationMessage(((ChannelOpenMessage)m).LocalChannelNumber, _remoteWindowSize, _remotePacketSize, _remoteChannelNumber))));
            _sessionMock.Setup(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()))
                .Callback<WaitHandle>(p => p.WaitOne(-1));

            var localPortEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            using (var localPortListener = new AsyncSocketListener(localPortEndPoint))
            {
                localPortListener.Start();

                localPortListener.Connected += socket =>
                    {
                        var channel = new ChannelDirectTcpip();
                        channel.Initialize(_sessionMock.Object, _localWindowSize, _localPacketSize);
                        channel.Open(_remoteHost, _port, _forwardedPortMock.Object, socket);

                        var closeForwardedPortThread =
                            new Thread(() =>
                                {
                                    // sleep for a short period to allow channel to actually start receiving from socket
                                    Thread.Sleep(1000);
                                    // raise Closing event on forwarded port
                                    _forwardedPortMock.Raise(p => p.Closing += null, EventArgs.Empty);
                                });
                        closeForwardedPortThread.Start();

                        channel.Bind();

                        closeForwardedPortThread.Join();
                    };

                var client = new Socket(localPortEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(localPortEndPoint);

                // attempt to receive from socket to verify it was shut down by forwarded port
                var buffer = new byte[16];
                var bytesReceived = client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                Assert.AreEqual(0, bytesReceived);
            }
        }
    }
}