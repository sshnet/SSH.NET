﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortLocalTest_Start_PortNeverStarted
    {
        private Mock<ISession> _sessionMock;
        private Mock<ISshConnectionInfo> _connectionInfoMock;
        private Mock<IChannelDirectTcpip> _channelMock;
        private ForwardedPortLocal _forwardedPort;
        private IPEndPoint _localEndpoint;
        private IPEndPoint _remoteEndpoint;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_forwardedPort != null)
            {
                _forwardedPort.Dispose();
                _forwardedPort = null;
            }
        }

        protected void Arrange()
        {
            var random = new Random();
            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();
            _localEndpoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"),
                random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));

            _connectionInfoMock = new Mock<ISshConnectionInfo>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelDirectTcpip>(MockBehavior.Strict);

            _ = _connectionInfoMock.Setup(p => p.Timeout)
                                   .Returns(TimeSpan.FromSeconds(15));
            _ = _sessionMock.Setup(p => p.IsConnected)
                            .Returns(true);
            _ = _sessionMock.Setup(p => p.ConnectionInfo)
                            .Returns(_connectionInfoMock.Object);

            _forwardedPort = new ForwardedPortLocal(_localEndpoint.Address.ToString(), (uint)_localEndpoint.Port,
                _remoteEndpoint.Address.ToString(), (uint)_remoteEndpoint.Port);
            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;
        }

        protected void Act()
        {
            _forwardedPort.Start();
        }

        [TestMethod]
        public void IsStartedShouldReturnTrue()
        {
            Assert.IsTrue(_forwardedPort.IsStarted);
        }

        [TestMethod]
        public void ForwardedPortShouldAcceptNewConnections()
        {
            Socket handlerSocket = null;

            _ = _sessionMock.Setup(p => p.CreateChannelDirectTcpip())
                            .Returns(_channelMock.Object);
            _ = _channelMock.Setup(p => p.Open(_forwardedPort.Host, _forwardedPort.Port, _forwardedPort, It.IsAny<Socket>()))
                            .Callback<string, uint, IForwardedPort, Socket>((address, port, forwardedPort, socket) => handlerSocket = socket);
            _ = _channelMock.Setup(p => p.Bind())
                            .Callback(() =>
                                {
                                    if (handlerSocket != null && handlerSocket.Connected)
                                    {
                                        handlerSocket.Shutdown(SocketShutdown.Both);
                                    }
                                });
            _ = _channelMock.Setup(p => p.Dispose());

            using (var client = new Socket(_localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                client.Connect(_localEndpoint);
            }
        }

        [TestMethod]
        public void ClosingShouldNotHaveFired()
        {
            Assert.AreEqual(0, _closingRegister.Count);
        }

        [TestMethod]
        public void ExceptionShouldNotHaveFired()
        {
            Assert.AreEqual(0, _exceptionRegister.Count);
        }
    }
}
