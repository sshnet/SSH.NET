using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class ProtocolVersionExchangeTest_ServerResponseValid_NoComments
    {
        private AsyncSocketListener _server;
        private TimeSpan _timeout;
        private IPEndPoint _serverEndPoint;
        private List<byte> _dataReceivedByServer;
        private byte[] _serverIdentification;
        private SshClient _client;
        private SshIdentification _actual;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _server?.Dispose();
            _server = null;

            _client?.Dispose();
            _client = null;
        }

        protected void Arrange()
        {
            _timeout = TimeSpan.FromSeconds(5);
            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _dataReceivedByServer = new List<byte>();
            _serverIdentification = Encoding.UTF8.GetBytes("Welcome stranger!\r\n\r\nSSH-2.0-OurSSHAppliance\r\n!");

            _server = new AsyncSocketListener(_serverEndPoint);
            _server.Start();
            _server.BytesReceived += (bytes, socket) =>
                {
                    var isFirstReceive = _dataReceivedByServer.Count == 0;

                    _dataReceivedByServer.AddRange(bytes);

                    if (isFirstReceive)
                    {
                        socket.Send(_serverIdentification);
                        socket.Shutdown(SocketShutdown.Send);
                    }
                };

            _client = new SshClient(_serverEndPoint.Address.ToString(), _serverEndPoint.Port, "user", "password");
            _client.ConnectionInfo.Timeout = _timeout;
        }

        protected void Act()
        {
            _client.ServerIdentificationReceived += (sender, e) => _actual = e.SshIdentification;

            // Since the server is not a full implementation, connecting fails after the
            // identification has been received.
            _ = Assert.ThrowsExactly<SshConnectionException>(_client.Connect);

            // Give some time to process all messages
            Thread.Sleep(200);
        }

        [TestMethod]
        public void ConnectShouldReturnIdentificationOfServer()
        {
            Assert.IsNotNull(_actual);
            Assert.AreEqual("2.0", _actual.ProtocolVersion);
            Assert.AreEqual("OurSSHAppliance", _actual.SoftwareVersion);
            Assert.IsNull(_actual.Comments);
        }

        [TestMethod]
        public void ClientIdentificationWasSentToServer()
        {
            Assert.SequenceStartsWith(Session.ClientVersionBytes, _dataReceivedByServer);
        }
    }
}
