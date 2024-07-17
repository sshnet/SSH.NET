﻿using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ProtocolVersionExchangeTest_ServerResponseValid_EmptySoftwareVersion
    {
        private AsyncSocketListener _server;
        private ProtocolVersionExchange _protocolVersionExchange;
        private string _clientVersion;
        private TimeSpan _timeout;
        private IPEndPoint _serverEndPoint;
        private List<byte> _dataReceivedByServer;
        private byte[] _serverIdentification;
        private bool _clientDisconnected;
        private Socket _client;
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
            _clientVersion = "SSH-2.0-Renci.SshNet.SshClient.0.0.1";
            _timeout = TimeSpan.FromSeconds(5);
            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _dataReceivedByServer = new List<byte>();
            _serverIdentification = Encoding.UTF8.GetBytes("SSH-78.5-\r\n");

            _server = new AsyncSocketListener(_serverEndPoint);
            _server.Start();
            _server.BytesReceived += (bytes, socket) =>
                {
                    _dataReceivedByServer.AddRange(bytes);

                    _ = socket.Send(_serverIdentification);

                    socket.Shutdown(SocketShutdown.Send);
                };
            _server.Disconnected += (socket) => _clientDisconnected = true;

            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(_serverEndPoint);

            _protocolVersionExchange = new ProtocolVersionExchange();
        }

        protected void Act()
        {
            _actual = _protocolVersionExchange.Start(_clientVersion, _client, _timeout);

            // Give some time to process all messages
            Thread.Sleep(200);
        }

        [TestMethod]
        public void StartShouldReturnIdentificationOfServer()
        {
            Assert.IsNotNull(_actual);
            Assert.AreEqual("78.5", _actual.ProtocolVersion);
            Assert.AreEqual(string.Empty, _actual.SoftwareVersion);
            Assert.IsNull(_actual.Comments);
        }

        [TestMethod]
        public void ClientIdentificationWasSentToServer()
        {
            var expected = Encoding.UTF8.GetBytes(_clientVersion);

            Assert.AreEqual(expected.Length + 2, _dataReceivedByServer.Count);

            Assert.IsTrue(expected.SequenceEqual(_dataReceivedByServer.Take(expected.Length)));
            Assert.AreEqual(Session.CarriageReturn, _dataReceivedByServer[_dataReceivedByServer.Count - 2]);
            Assert.AreEqual(Session.LineFeed, _dataReceivedByServer[_dataReceivedByServer.Count - 1]);
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
