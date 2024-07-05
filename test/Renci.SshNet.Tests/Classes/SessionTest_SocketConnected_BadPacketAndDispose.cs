using System;
using System.Net;
using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Common;
using Renci.SshNet.Connection;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_SocketConnected_BadPacketAndDispose
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISocketFactory> _socketFactoryMock;
        private Mock<IConnector> _connectorMock;
        private Mock<IProtocolVersionExchange> _protocolVersionExchangeMock;
        private ConnectionInfo _connectionInfo;
        private Session _session;
        private AsyncSocketListener _serverListener;
        private IPEndPoint _serverEndPoint;
        private Socket _serverSocket;
        private Socket _clientSocket;
        private SshConnectionException _actualException;
        private SocketFactory _socketFactory;
        private ServiceFactory _serviceFactory;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void TearDown()
        {
            _serverListener?.Dispose();
        }

        protected void CreateMocks()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _socketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
            _connectorMock = new Mock<IConnector>(MockBehavior.Strict);
            _protocolVersionExchangeMock = new Mock<IProtocolVersionExchange>(MockBehavior.Strict);
        }

        protected void SetupData()
        {
            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 0);

            _serverListener = new AsyncSocketListener(_serverEndPoint);
            _serverListener.Connected += (socket) =>
                {
                    _serverSocket = socket;

                    // Since we're mocking the protocol version exchange, we can immediately send the bad
                    // packet upon establishing the connection

                    var badPacket = new byte[] { 0x0a, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05 };

                    _ = _serverSocket.Send(badPacket, 0, badPacket.Length, SocketFlags.None);

                    _serverSocket.Shutdown(SocketShutdown.Send);
                };
            _serverListener.Start();
            _serverEndPoint.Port = ((IPEndPoint)_serverListener.ListenerEndPoint).Port;

            _connectionInfo = new ConnectionInfo(_serverEndPoint.Address.ToString(), _serverEndPoint.Port, "user", new PasswordAuthenticationMethod("user", "password"))
            {
                Timeout = TimeSpan.FromMilliseconds(200)
            };
            _actualException = null;
            _socketFactory = new SocketFactory();
            _serviceFactory = new ServiceFactory();

            _session = new Session(_connectionInfo, _serviceFactoryMock.Object, _socketFactoryMock.Object);

            _clientSocket = new DirectConnector(_serviceFactory, _socketFactory).Connect(_connectionInfo);
        }

        protected void SetupMocks()
        {
            _ = _serviceFactoryMock.Setup(p => p.CreateConnector(_connectionInfo, _socketFactoryMock.Object))
                                   .Returns(_connectorMock.Object);
            _ = _connectorMock.Setup(p => p.Connect(_connectionInfo))
                              .Returns(_clientSocket);
            _ = _connectorMock.Setup(p => p.Dispose());
            _ = _serviceFactoryMock.Setup(p => p.CreateProtocolVersionExchange())
                                   .Returns(_protocolVersionExchangeMock.Object);
            _ = _protocolVersionExchangeMock.Setup(p => p.Start(_session.ClientVersion, _clientSocket, _connectionInfo.Timeout))
                                            .Returns(new SshIdentification("2.0", "XXX"));
        }

        protected void Arrange()
        {
            CreateMocks();
            SetupData();
            SetupMocks();
        }

        protected virtual void Act()
        {
            try
            {
                _session.Connect();
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void ConnectShouldThrowSshConnectionException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual(DisconnectReason.ProtocolError, _actualException.DisconnectReason);
            Assert.AreEqual("Bad packet length: 168101125.", _actualException.Message);
        }
    }
}
