using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using Renci.SshNet.Connection;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connected_ServerAndClientDisconnectRace
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISocketFactory> _socketFactoryMock;
        private Mock<IConnector> _connectorMock;
        private Mock<IProtocolVersionExchange> _protocolVersionExchangeMock;
        private Mock<IKeyExchange> _keyExchangeMock;
        private Mock<IClientAuthentication> _clientAuthenticationMock;
        private IPEndPoint _serverEndPoint;
        private string _keyExchangeAlgorithm;
        private DisconnectMessage _disconnectMessage;
        private SocketFactory _socketFactory;
        private ServiceFactory _serviceFactory;
        private bool _authenticationStarted;

        protected Random Random { get; private set; }
        protected byte[] SessionId { get; private set; }
        protected ConnectionInfo ConnectionInfo { get; private set; }
        protected IList<EventArgs> DisconnectedRegister { get; private set; }
        protected IList<MessageEventArgs<DisconnectMessage>> DisconnectReceivedRegister { get; private set; }
        protected IList<ExceptionEventArgs> ErrorOccurredRegister { get; private set; }
        protected AsyncSocketListener ServerListener { get; private set; }
        protected IList<byte[]> ServerBytesReceivedRegister { get; private set; }
        protected Session Session { get; private set; }
        protected Socket ClientSocket { get; private set; }
        protected Socket ServerSocket { get; private set; }
        internal SshIdentification ServerIdentification { get; private set; }

        private void TearDown()
        {
            ServerListener?.Dispose();
            Session?.Dispose();

            if (ClientSocket != null && ClientSocket.Connected)
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Dispose();
            }
        }

        protected virtual void SetupData()
        {
            Random = new Random();

            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 0);

            ServerListener = new AsyncSocketListener(_serverEndPoint);
            ServerListener.Connected += socket =>
                {
                    ServerSocket = socket;

                    // Since we're mocking the protocol version exchange, we'll immediately stat KEX upon
                    // having established the connection instead of when the client has been identified

                    var keyExchangeInitMessage = new KeyExchangeInitMessage
                    {
                        CompressionAlgorithmsClientToServer = new string[0],
                        CompressionAlgorithmsServerToClient = new string[0],
                        EncryptionAlgorithmsClientToServer = new string[0],
                        EncryptionAlgorithmsServerToClient = new string[0],
                        KeyExchangeAlgorithms = new[] { _keyExchangeAlgorithm },
                        LanguagesClientToServer = new string[0],
                        LanguagesServerToClient = new string[0],
                        MacAlgorithmsClientToServer = new string[0],
                        MacAlgorithmsServerToClient = new string[0],
                        ServerHostKeyAlgorithms = new string[0]
                    };
                    var keyExchangeInit = keyExchangeInitMessage.GetPacket(8, null);
                    _ = ServerSocket.Send(keyExchangeInit, 4, keyExchangeInit.Length - 4, SocketFlags.None);
                };
            ServerListener.BytesReceived += (received, socket) =>
                {
                    ServerBytesReceivedRegister.Add(received);
                };

            ServerListener.Start();

            _serverEndPoint.Port = ((IPEndPoint)ServerListener.ListenerEndPoint).Port;

            ConnectionInfo = new ConnectionInfo(
                _serverEndPoint.Address.ToString(),
                _serverEndPoint.Port,
                "user",
                new PasswordAuthenticationMethod("user", "password"))
            { Timeout = TimeSpan.FromSeconds(20) };
            _keyExchangeAlgorithm = Random.Next().ToString(CultureInfo.InvariantCulture);
            SessionId = new byte[10];
            Random.NextBytes(SessionId);
            DisconnectedRegister = new List<EventArgs>();
            DisconnectReceivedRegister = new List<MessageEventArgs<DisconnectMessage>>();
            ErrorOccurredRegister = new List<ExceptionEventArgs>();
            ServerBytesReceivedRegister = new List<byte[]>();
            ServerIdentification = new SshIdentification("2.0", "OurServerStub");
            _authenticationStarted = false;
            _disconnectMessage = new DisconnectMessage(DisconnectReason.ServiceNotAvailable, "Not today!");
            _socketFactory = new SocketFactory();
            _serviceFactory = new ServiceFactory();

            Session = new Session(ConnectionInfo, _serviceFactoryMock.Object, _socketFactoryMock.Object);
            Session.Disconnected += (sender, args) => DisconnectedRegister.Add(args);
            Session.DisconnectReceived += (sender, args) => DisconnectReceivedRegister.Add(args);
            Session.ErrorOccured += (sender, args) => ErrorOccurredRegister.Add(args);
            Session.KeyExchangeInitReceived += (sender, args) =>
            {
                var newKeysMessage = new NewKeysMessage();
                var newKeys = newKeysMessage.GetPacket(8, null);
                _ = ServerSocket.Send(newKeys, 4, newKeys.Length - 4, SocketFlags.None);

                if (!_authenticationStarted)
                {
                    var serviceAcceptMessage = ServiceAcceptMessageBuilder.Create(ServiceName.UserAuthentication).Build();
                    _ = ServerSocket.Send(serviceAcceptMessage, 0, serviceAcceptMessage.Length, SocketFlags.None);
                    _authenticationStarted = true;
                }
            };

            ClientSocket = new DirectConnector(_serviceFactory, _socketFactory).Connect(ConnectionInfo);
        }

        private void CreateMocks()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _socketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
            _connectorMock = new Mock<IConnector>(MockBehavior.Strict);
            _protocolVersionExchangeMock = new Mock<IProtocolVersionExchange>(MockBehavior.Strict);
            _keyExchangeMock = new Mock<IKeyExchange>(MockBehavior.Strict);
            _clientAuthenticationMock = new Mock<IClientAuthentication>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            _ = _serviceFactoryMock.Setup(p => p.CreateConnector(ConnectionInfo, _socketFactoryMock.Object))
                                   .Returns(_connectorMock.Object);
            _ = _connectorMock.Setup(p => p.Connect(ConnectionInfo))
                              .Returns(ClientSocket);
            _ = _connectorMock.Setup(p => p.Dispose());
            _ = _serviceFactoryMock.Setup(p => p.CreateProtocolVersionExchange())
                                   .Returns(_protocolVersionExchangeMock.Object);
            _ = _protocolVersionExchangeMock.Setup(p => p.Start(Session.ClientVersion, ClientSocket, ConnectionInfo.Timeout))
                                            .Returns(ServerIdentification);
            _ = _serviceFactoryMock.Setup(p => p.CreateKeyExchange(ConnectionInfo.KeyExchangeAlgorithms, new[] { _keyExchangeAlgorithm }))
                                   .Returns(_keyExchangeMock.Object);
            _ = _keyExchangeMock.Setup(p => p.Name)
                                .Returns(_keyExchangeAlgorithm);
            _ = _keyExchangeMock.Setup(p => p.Start(Session, It.IsAny<KeyExchangeInitMessage>(), false));
            _ = _keyExchangeMock.Setup(p => p.ExchangeHash)
                                .Returns(SessionId);
            _ = _keyExchangeMock.Setup(p => p.CreateServerCipher(out It.Ref<bool>.IsAny))
                                .Returns((ref bool serverAead) =>
                                {
                                    serverAead = false;
                                    return (Cipher)null;
                                });
            _ = _keyExchangeMock.Setup(p => p.CreateClientCipher(out It.Ref<bool>.IsAny))
                                .Returns((ref bool clientAead) =>
                                {
                                    clientAead = false;
                                    return (Cipher)null;
                                });
            _ = _keyExchangeMock.Setup(p => p.CreateServerHash(out It.Ref<bool>.IsAny))
                                .Returns((ref bool serverEtm) =>
                                {
                                    serverEtm = false;
                                    return (HashAlgorithm)null;
                                });
            _ = _keyExchangeMock.Setup(p => p.CreateClientHash(out It.Ref<bool>.IsAny))
                                .Returns((ref bool clientEtm) =>
                                {
                                    clientEtm = false;
                                    return (HashAlgorithm)null;
                                });
            _ = _keyExchangeMock.Setup(p => p.CreateCompressor())
                                .Returns((Compressor)null);
            _ = _keyExchangeMock.Setup(p => p.CreateDecompressor())
                                .Returns((Compressor)null);
            _ = _keyExchangeMock.Setup(p => p.Dispose());
            _ = _serviceFactoryMock.Setup(p => p.CreateClientAuthentication())
                                   .Returns(_clientAuthenticationMock.Object);
            _ = _clientAuthenticationMock.Setup(p => p.Authenticate(ConnectionInfo, Session));
        }

        protected virtual void Arrange()
        {
            CreateMocks();
            SetupData();
            SetupMocks();

            Session.Connect();
        }

        [TestMethod]
        public void Act()
        {
            for (var i = 0; i < 50; i++)
            {
                Arrange();
                try
                {
                    var disconnect = _disconnectMessage.GetPacket(8, null);
                    _ = ServerSocket.Send(disconnect, 4, disconnect.Length - 4, SocketFlags.None);
                    Session.Disconnect();
                }
                finally
                {
                    TearDown();
                }
            }
        }

        private class ServiceAcceptMessageBuilder
        {
            private readonly ServiceName _serviceName;

            private ServiceAcceptMessageBuilder(ServiceName serviceName)
            {
                _serviceName = serviceName;
            }

            public static ServiceAcceptMessageBuilder Create(ServiceName serviceName)
            {
                return new ServiceAcceptMessageBuilder(serviceName);
            }

            public byte[] Build()
            {
                var serviceName = _serviceName.ToArray();
                var target = new ServiceAcceptMessage();

                var sshDataStream = new SshDataStream(4 + 1 + 1 + 4 + serviceName.Length);
                sshDataStream.Write((uint)(sshDataStream.Capacity - 4)); // packet length
                sshDataStream.WriteByte(0); // padding length
                sshDataStream.WriteByte(target.MessageNumber);
                sshDataStream.WriteBinary(serviceName);
                return sshDataStream.ToArray();
            }
        }
    }
}
