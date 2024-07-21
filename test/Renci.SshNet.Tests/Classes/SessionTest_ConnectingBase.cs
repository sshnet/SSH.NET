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
    public abstract class SessionTest_ConnectingBase
    {
        internal Mock<IServiceFactory> ServiceFactoryMock { get; private set; }
        internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
        internal Mock<IConnector> ConnectorMock { get; private set; }

        private Mock<IProtocolVersionExchange> _protocolVersionExchangeMock;
        private Mock<IKeyExchange> _keyExchangeMock;
        private Mock<IClientAuthentication> _clientAuthenticationMock;
        private IPEndPoint _serverEndPoint;
        private string[] _keyExchangeAlgorithms;
        private bool _authenticationStarted;
        private SocketFactory _socketFactory;

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
        protected SshIdentification ServerIdentification { get; set; }
        protected virtual bool ServerSupportsStrictKex { get; }

        protected virtual bool ServerResetsSequenceAfterSendingNewKeys
        {
            get
            {
                return ServerSupportsStrictKex;
            }
        }

        protected uint ServerOutboundPacketSequence { get; set; }

        [TestInitialize]
        public void Setup()
        {
            CreateMocks();
            SetupData();
            SetupMocks();
        }

        protected virtual void ActionBeforeKexInit()
        {
        }

        protected virtual void ActionAfterKexInit()
        {
        }

        [TestCleanup]
        public void TearDown()
        {
            if (ServerListener != null)
            {
                ServerListener.Dispose();
                ServerListener = null;
            }

            if (ServerSocket != null)
            {
                ServerSocket.Dispose();
                ServerSocket = null;
            }

            if (Session != null)
            {
                Session.Dispose();
                Session = null;
            }

            if (ClientSocket != null && ClientSocket.Connected)
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Dispose();
            }
        }

        protected virtual void SetupData()
        {
            Random = new Random();

            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            ConnectionInfo = new ConnectionInfo(
                _serverEndPoint.Address.ToString(),
                _serverEndPoint.Port,
                "user",
                new PasswordAuthenticationMethod("user", "password"))
            { Timeout = TimeSpan.FromSeconds(20) };
            _keyExchangeAlgorithms = ServerSupportsStrictKex ?
                                     [Random.Next().ToString(CultureInfo.InvariantCulture), "kex-strict-s-v00@openssh.com"] :
                                     [Random.Next().ToString(CultureInfo.InvariantCulture)];
            SessionId = new byte[10];
            Random.NextBytes(SessionId);
            DisconnectedRegister = new List<EventArgs>();
            DisconnectReceivedRegister = new List<MessageEventArgs<DisconnectMessage>>();
            ErrorOccurredRegister = new List<ExceptionEventArgs>();
            ServerBytesReceivedRegister = new List<byte[]>();
            ServerIdentification = new SshIdentification("2.0", "OurServerStub");
            _authenticationStarted = false;
            _socketFactory = new SocketFactory();

            Session = new Session(ConnectionInfo, ServiceFactoryMock.Object, SocketFactoryMock.Object);
            Session.Disconnected += (sender, args) => DisconnectedRegister.Add(args);
            Session.DisconnectReceived += (sender, args) => DisconnectReceivedRegister.Add(args);
            Session.ErrorOccured += (sender, args) => ErrorOccurredRegister.Add(args);

            ServerListener = new AsyncSocketListener(_serverEndPoint)
            {
                ShutdownRemoteCommunicationSocket = false
            };
            ServerListener.Connected += socket =>
            {
                ServerSocket = socket;
                ActionBeforeKexInit();
                var keyExchangeInitMessage = new KeyExchangeInitMessage
                {
                    CompressionAlgorithmsClientToServer = new string[0],
                    CompressionAlgorithmsServerToClient = new string[0],
                    EncryptionAlgorithmsClientToServer = new string[0],
                    EncryptionAlgorithmsServerToClient = new string[0],
                    KeyExchangeAlgorithms = _keyExchangeAlgorithms,
                    LanguagesClientToServer = new string[0],
                    LanguagesServerToClient = new string[0],
                    MacAlgorithmsClientToServer = new string[0],
                    MacAlgorithmsServerToClient = new string[0],
                    ServerHostKeyAlgorithms = new string[0]
                };
                var keyExchangeInit = keyExchangeInitMessage.GetPacket(8, null);
                _ = ServerSocket.Send(keyExchangeInit, 4, keyExchangeInit.Length - 4, SocketFlags.None);
                ServerOutboundPacketSequence++;
            };
            ServerListener.BytesReceived += (received, socket) =>
            {
                ServerBytesReceivedRegister.Add(received);

                if (received.Length > 5 && received[5] == 20)
                {
                    ActionAfterKexInit();
                    var newKeysMessage = new NewKeysMessage();
                    var newKeys = newKeysMessage.GetPacket(8, null);
                    _ = ServerSocket.Send(newKeys, 4, newKeys.Length - 4, SocketFlags.None);

                    if (ServerResetsSequenceAfterSendingNewKeys)
                    {
                        ServerOutboundPacketSequence = 0;
                    }
                    else
                    {
                        ServerOutboundPacketSequence++;
                    }

                    if (!_authenticationStarted)
                    {
                        var serviceAcceptMessage = ServiceAcceptMessageBuilder.Create(ServiceName.UserAuthentication)
                                                                              .Build(ServerOutboundPacketSequence);

#if NET6_0_OR_GREATER
                        var hash = SHA256.HashData(serviceAcceptMessage);
#else
                        var hash = SHA256.Create().ComputeHash(serviceAcceptMessage);
#endif

                        var packet = new byte[serviceAcceptMessage.Length - 4 + hash.Length];

                        Array.Copy(serviceAcceptMessage, 4, packet, 0, serviceAcceptMessage.Length - 4);
                        Array.Copy(hash, 0, packet, serviceAcceptMessage.Length - 4, hash.Length);

                        _ = ServerSocket.Send(packet, 0, packet.Length, SocketFlags.None);

                        ServerOutboundPacketSequence++;

                        _authenticationStarted = true;
                    }
                }
            };
            ServerListener.Start();

            ClientSocket = new DirectConnector(_socketFactory).Connect(ConnectionInfo);
        }

        private void CreateMocks()
        {
            ServiceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
            ConnectorMock = new Mock<IConnector>(MockBehavior.Strict);
            _protocolVersionExchangeMock = new Mock<IProtocolVersionExchange>(MockBehavior.Strict);
            _keyExchangeMock = new Mock<IKeyExchange>(MockBehavior.Strict);
            _clientAuthenticationMock = new Mock<IClientAuthentication>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            _ = ServiceFactoryMock.Setup(p => p.CreateConnector(ConnectionInfo, SocketFactoryMock.Object))
                                  .Returns(ConnectorMock.Object);
            _ = ConnectorMock.Setup(p => p.Connect(ConnectionInfo))
                             .Returns(ClientSocket);
            _ = ServiceFactoryMock.Setup(p => p.CreateProtocolVersionExchange())
                                  .Returns(_protocolVersionExchangeMock.Object);
            _ = _protocolVersionExchangeMock.Setup(p => p.Start(Session.ClientVersion, ClientSocket, ConnectionInfo.Timeout))
                                            .Returns(() => ServerIdentification);
            _ = ServiceFactoryMock.Setup(p => p.CreateKeyExchange(ConnectionInfo.KeyExchangeAlgorithms, _keyExchangeAlgorithms)).Returns(_keyExchangeMock.Object);

            _ = _keyExchangeMock.Setup(p => p.Name)
                                .Returns(_keyExchangeAlgorithms[0]);
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
                                    return SHA256.Create();
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
            _ = ServiceFactoryMock.Setup(p => p.CreateClientAuthentication())
                                  .Returns(_clientAuthenticationMock.Object);
            _ = _clientAuthenticationMock.Setup(p => p.Authenticate(ConnectionInfo, Session));
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

            public byte[] Build(uint sequence)
            {
                var serviceName = _serviceName.ToArray();
                var target = new ServiceAcceptMessage();

                var sshDataStream = new SshDataStream(4 + 4 + 1 + 1 + 4 + serviceName.Length);
                sshDataStream.Write(sequence);
                sshDataStream.Write((uint)(sshDataStream.Capacity - 8)); //sequence and packet length
                sshDataStream.WriteByte(0); // padding length
                sshDataStream.WriteByte(target.MessageNumber);
                sshDataStream.WriteBinary(serviceName);
                return sshDataStream.ToArray();
            }
        }
    }
}
