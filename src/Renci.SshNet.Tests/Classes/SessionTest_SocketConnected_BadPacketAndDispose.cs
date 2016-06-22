using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_SocketConnected_BadPacketAndDispose
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private ConnectionInfo _connectionInfo;
        private Session _session;
        private AsyncSocketListener _serverListener;
        private IPEndPoint _serverEndPoint;
        private Socket _serverSocket;
        private SshConnectionException _actualException;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_serverListener != null)
            {
                _serverListener.Dispose();
            }
        }

        protected void Arrange()
        {
            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _connectionInfo = new ConnectionInfo(
                _serverEndPoint.Address.ToString(),
                _serverEndPoint.Port,
                "user",
                new PasswordAuthenticationMethod("user", "password"));
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(200);
            _actualException = null;

            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);

            _serverListener = new AsyncSocketListener(_serverEndPoint);
            _serverListener.Connected += (socket) =>
                {
                    _serverSocket = socket;

                    socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("SSH-2.0-SshStub\r\n"));
                };
            _serverListener.BytesReceived += (received, socket) =>
                {
                    var badPacket = new byte[] {0x0a, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05};
                    _serverSocket.Send(badPacket, 0, badPacket.Length, SocketFlags.None);
                    _serverSocket.Shutdown(SocketShutdown.Send);
                };
            _serverListener.Start();
        }

        protected virtual void Act()
        {
            try
            {
                using (_session = new Session(_connectionInfo, _serviceFactoryMock.Object))
                {
                    _session.Connect();
                }
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
