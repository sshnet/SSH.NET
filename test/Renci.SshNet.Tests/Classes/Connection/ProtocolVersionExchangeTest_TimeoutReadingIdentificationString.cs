using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class ProtocolVersionExchangeTest_TimeoutReadingIdentificationString
    {
        private AsyncSocketListener _server;
        private TimeSpan _timeout;
        private IPEndPoint _serverEndPoint;
        private List<byte> _dataReceivedByServer;
        private SshClient _client;
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
            _server?.Dispose();
            _server = null;

            _client?.Dispose();
            _client = null;
        }

        protected void Arrange()
        {
            _timeout = TimeSpan.FromMilliseconds(200);
            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _dataReceivedByServer = new List<byte>();

            _server = new AsyncSocketListener(_serverEndPoint);
            _server.Start();
            _server.BytesReceived += (bytes, socket) =>
                {
                    _dataReceivedByServer.AddRange(bytes);

                    _ = socket.Send(Encoding.UTF8.GetBytes("Welcome!\r\n"));
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
            catch (SshOperationTimeoutException ex)
            {
                _actualException = ex;
            }

            // Give some time to process all messages
            Thread.Sleep(200);
        }

        [TestMethod]
        public void ConnectShouldHaveThrownSshOperationTimeoutException()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreEqual(string.Format("Socket read operation has timed out after {0} milliseconds.", _timeout.TotalMilliseconds), _actualException.Message);
        }

        [TestMethod]
        public void ClientIdentificationWasSentToServer()
        {
            Assert.SequenceStartsWith(Session.ClientVersionBytes, _dataReceivedByServer);
        }
    }
}
