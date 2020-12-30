using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class ProtocolVersionExchangeTest_ConnectionClosedByServer_NoDataSentByServer
    {
        private AsyncSocketListener _server;
        private ProtocolVersionExchange _protocolVersionExchange;
        private string _clientVersion;
        private TimeSpan _timeout;
        private IPEndPoint _serverEndPoint;
        private List<byte> _dataReceivedByServer;
        private bool _clientDisconnected;
        private Socket _client;
        private SshConnectionException _actualException;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_server != null)
            {
                _server.Dispose();
                _server = null;
            }

            if (_client != null)
            {
                _client.Shutdown(SocketShutdown.Both);
                _client.Close();
                _client = null;
            }
        }

        protected void Arrange()
        {
            _clientVersion = "\uD55C";
            _timeout = TimeSpan.FromSeconds(5);
            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _dataReceivedByServer = new List<byte>();

            _server = new AsyncSocketListener(_serverEndPoint);
            _server.Start();
            _server.BytesReceived += (bytes, socket) =>
                {
                    _dataReceivedByServer.AddRange(bytes);
                    socket.Shutdown(SocketShutdown.Send);
                };
            _server.Disconnected += (socket) => _clientDisconnected = true;

            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(_serverEndPoint);

            _protocolVersionExchange = new ProtocolVersionExchange();
        }

        protected void Act()
        {
            try
            {
                _protocolVersionExchange.Start(_clientVersion, _client, _timeout);
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void StartShouldHaveThrownSshConnectionException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual(string.Format("The server response does not contain an SSH identification string.{0}" +
                                          "The connection to the remote server was closed before any data was received.{0}{0}" +
                                          "More information on the Protocol Version Exchange is available here:{0}" +
                                          "https://tools.ietf.org/html/rfc4253#section-4.2",
                                          Environment.NewLine),
                            _actualException.Message);
        }

        [TestMethod]
        public void ClientIdentificationWasSentToServer()
        {
            Assert.AreEqual(5, _dataReceivedByServer.Count);

            Assert.AreEqual(0xed, _dataReceivedByServer[0]);
            Assert.AreEqual(0x95, _dataReceivedByServer[1]);
            Assert.AreEqual(0x9c, _dataReceivedByServer[2]);
            Assert.AreEqual(0x0d, _dataReceivedByServer[3]);
            Assert.AreEqual(0x0a, _dataReceivedByServer[4]);
        }

        [TestMethod]
        public void ConnectionIsClosedByServer()
        {
            Assert.IsTrue(_client.Connected);
            Assert.IsFalse(_clientDisconnected);

            var bytesReceived = _client.Receive(new byte[1]);
            Assert.AreEqual(0, bytesReceived);

            Assert.IsTrue(_client.Connected);
            Assert.IsFalse(_clientDisconnected);
        }
    }
}
