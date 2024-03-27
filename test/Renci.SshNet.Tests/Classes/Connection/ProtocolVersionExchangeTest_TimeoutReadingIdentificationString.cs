using System;
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
    public class ProtocolVersionExchangeTest_TimeoutReadingIdentificationString
    {
        private AsyncSocketListener _server;
        private ProtocolVersionExchange _protocolVersionExchange;
        private string _clientVersion;
        private TimeSpan _timeout;
        private IPEndPoint _serverEndPoint;
        private List<byte> _dataReceivedByServer;
        private bool _clientDisconnected;
        private Socket _client;
        private SshOperationTimeoutException _actualException;

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
                _client.Close();
                _client = null;
            }
        }

        protected void Arrange()
        {
            _clientVersion = "SSH-2.0-Renci.SshNet.SshClient.0.0.1";
            _timeout = TimeSpan.FromMilliseconds(200);
            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _dataReceivedByServer = new List<byte>();
            _clientDisconnected = false;

            _server = new AsyncSocketListener(_serverEndPoint);
            _server.Start();
            _server.BytesReceived += (bytes, socket) =>
                {
                    _dataReceivedByServer.AddRange(bytes);

                    _ = socket.Send(Encoding.UTF8.GetBytes("Welcome!\r\n"));
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
                _ = _protocolVersionExchange.Start(_clientVersion, _client, _timeout);
                Assert.Fail();
            }
            catch (SshOperationTimeoutException ex)
            {
                _actualException = ex;
            }

            // Give some time to process all messages
            Thread.Sleep(200);
        }

        [TestMethod]
        public void StartShouldHaveThrownSshOperationTimeoutException()
        {
            Assert.IsInstanceOfType<SshOperationTimeoutException>(_actualException);
            Assert.IsInstanceOfType<SocketException>(_actualException.InnerException);
            Assert.AreEqual(string.Format("Socket read operation has timed out after {0} milliseconds.", _timeout.TotalMilliseconds), _actualException.Message);
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
        public void ClientSocketShouldBeConnected()
        {
            Assert.IsTrue(_client.Connected);
            Assert.IsFalse(_clientDisconnected);
        }
    }
}
