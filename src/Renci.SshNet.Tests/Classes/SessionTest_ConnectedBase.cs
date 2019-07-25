using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public abstract class SessionTest_ConnectedBase
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<IKeyExchange> _keyExchangeMock;
        private Mock<IClientAuthentication> _clientAuthenticationMock;
        private IPEndPoint _serverEndPoint;
        private string _keyExchangeAlgorithm;

        protected Random Random { get; private set; }
        protected byte[] SessionId { get; private set; }
        protected ConnectionInfo ConnectionInfo { get; private set; }
        protected IList<EventArgs> DisconnectedRegister { get; private set; }
        protected IList<MessageEventArgs<DisconnectMessage>> DisconnectReceivedRegister { get; private set; }
        protected IList<ExceptionEventArgs> ErrorOccurredRegister { get; private set; }
        protected AsyncSocketListener ServerListener { get; private set; }
        protected IList<byte[]> ServerBytesReceivedRegister { get; private set; }
        protected Session Session { get; private set; }
        protected Socket ServerSocket { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void TearDown()
        {
            if (ServerSocket != null)
            {
                ServerSocket.Dispose();
                ServerSocket = null;
            }

            if (ServerListener != null)
            {
                ServerListener.Dispose();
                ServerListener = null;
            }

            if (Session != null)
            {
                Session.Dispose();
                Session = null;
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
            {Timeout = TimeSpan.FromSeconds(20)};
            _keyExchangeAlgorithm = Random.Next().ToString(CultureInfo.InvariantCulture);
            SessionId = new byte[10];
            Random.NextBytes(SessionId);
            DisconnectedRegister = new List<EventArgs>();
            DisconnectReceivedRegister = new List<MessageEventArgs<DisconnectMessage>>();
            ErrorOccurredRegister = new List<ExceptionEventArgs>();
            ServerBytesReceivedRegister = new List<byte[]>();

            Session = new Session(ConnectionInfo, _serviceFactoryMock.Object);
            Session.Disconnected += (sender, args) => DisconnectedRegister.Add(args);
            Session.DisconnectReceived += (sender, args) => DisconnectReceivedRegister.Add(args);
            Session.ErrorOccured += (sender, args) => ErrorOccurredRegister.Add(args);
            Session.KeyExchangeInitReceived += (sender, args) =>
                {
                    var newKeysMessage = new NewKeysMessage();
                    var newKeys = newKeysMessage.GetPacket(8, null);
                    ServerSocket.Send(newKeys, 4, newKeys.Length - 4, SocketFlags.None);
                };

            ServerListener = new AsyncSocketListener(_serverEndPoint);
            ServerListener.Connected += socket =>
                {
                    ServerSocket = socket;

                    socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("SSH-2.0-SshStub\r\n"));
                };

            var counter = 0;

            ServerListener.BytesReceived += (received, socket) =>
                {
                    ServerBytesReceivedRegister.Add(received);

                    switch (counter++)
                    {
                        case 0:
                            var keyExchangeInitMessage = new KeyExchangeInitMessage
                            {
                                CompressionAlgorithmsClientToServer = new string[0],
                                CompressionAlgorithmsServerToClient = new string[0],
                                EncryptionAlgorithmsClientToServer = new string[0],
                                EncryptionAlgorithmsServerToClient = new string[0],
                                KeyExchangeAlgorithms = new[] {_keyExchangeAlgorithm},
                                LanguagesClientToServer = new string[0],
                                LanguagesServerToClient = new string[0],
                                MacAlgorithmsClientToServer = new string[0],
                                MacAlgorithmsServerToClient = new string[0],
                                ServerHostKeyAlgorithms = new string[0]
                            };
                            var keyExchangeInit = keyExchangeInitMessage.GetPacket(8, null);
                            ServerSocket.Send(keyExchangeInit, 4, keyExchangeInit.Length - 4, SocketFlags.None);
                            break;
                        case 1:
                            var serviceAcceptMessage =
                                ServiceAcceptMessageBuilder.Create(ServiceName.UserAuthentication)
                                    .Build();
                            ServerSocket.Send(serviceAcceptMessage, 0, serviceAcceptMessage.Length, SocketFlags.None);
                            break;
                    }
                };
        }

        private void CreateMocks()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _keyExchangeMock = new Mock<IKeyExchange>(MockBehavior.Strict);
            _clientAuthenticationMock = new Mock<IClientAuthentication>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            _serviceFactoryMock.Setup(
                p =>
                    p.CreateKeyExchange(ConnectionInfo.KeyExchangeAlgorithms, new[] { _keyExchangeAlgorithm })).Returns(_keyExchangeMock.Object);
            _keyExchangeMock.Setup(p => p.Name).Returns(_keyExchangeAlgorithm);
            _keyExchangeMock.Setup(p => p.Start(Session, It.IsAny<KeyExchangeInitMessage>()));
            _keyExchangeMock.Setup(p => p.ExchangeHash).Returns(SessionId);
            _keyExchangeMock.Setup(p => p.CreateServerCipher()).Returns((Cipher) null);
            _keyExchangeMock.Setup(p => p.CreateClientCipher()).Returns((Cipher) null);
            _keyExchangeMock.Setup(p => p.CreateServerHash()).Returns((HashAlgorithm) null);
            _keyExchangeMock.Setup(p => p.CreateClientHash()).Returns((HashAlgorithm) null);
            _keyExchangeMock.Setup(p => p.CreateCompressor()).Returns((Compressor) null);
            _keyExchangeMock.Setup(p => p.CreateDecompressor()).Returns((Compressor) null);
            _keyExchangeMock.Setup(p => p.Dispose());
            _serviceFactoryMock.Setup(p => p.CreateClientAuthentication())
                .Callback(ClientAuthentication_Callback)
                .Returns(_clientAuthenticationMock.Object);
            _clientAuthenticationMock.Setup(p => p.Authenticate(ConnectionInfo, Session));
        }

        protected virtual void Arrange()
        {
            CreateMocks();
            SetupData();
            SetupMocks();

            ServerListener.Start();
            Session.Connect();
        }

        protected virtual void ClientAuthentication_Callback()
        {
        }

        protected abstract void Act();

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

                var sshDataStream = new SshDataStream(4 + 1 + 1 + 4 + serviceName.Length);
                sshDataStream.Write((uint)(sshDataStream.Capacity - 4)); // packet length
                sshDataStream.WriteByte(0); // padding length
                sshDataStream.WriteByte(ServiceAcceptMessage.MessageNumber);
                sshDataStream.WriteBinary(serviceName);
                return sshDataStream.ToArray();
            }
        }
    }
}
