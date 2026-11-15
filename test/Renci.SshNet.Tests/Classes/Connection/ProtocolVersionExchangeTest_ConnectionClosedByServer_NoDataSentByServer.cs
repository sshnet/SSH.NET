using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class ProtocolVersionExchangeTest_ConnectionClosedByServer_NoDataSentByServer
    {
        private AsyncSocketListener _server;
        private TimeSpan _timeout;
        private IPEndPoint _serverEndPoint;
        private List<byte> _dataReceivedByServer;
        private SshClient _client;
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

            _server = new AsyncSocketListener(_serverEndPoint);
            _server.Start();
            _server.BytesReceived += (bytes, socket) =>
                {
                    _dataReceivedByServer.AddRange(bytes);
                    socket.Shutdown(SocketShutdown.Send);
                };

            _client = new SshClient(_serverEndPoint.Address.ToString(), _serverEndPoint.Port, "user", "password");
            _client.ConnectionInfo.Timeout = _timeout;
        }

        protected void Act()
        {
            try
            {
                _client.Connect();
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
                _actualException = ex;
            }

            // Give some time to process all messages
            Thread.Sleep(200);
        }

        [TestMethod]
        public void ConnectShouldHaveThrownSshConnectionException()
        {
            Assert.IsInstanceOfType<SshConnectionException>(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("The connection to the remote server was closed before a valid SSH identification string was received.", _actualException.Message);
        }

        [TestMethod]
        public void ClientIdentificationWasSentToServer()
        {
            Assert.AreSequenceEqual(Session.ClientVersionBytes, _dataReceivedByServer);
        }
    }
}
