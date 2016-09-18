﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortDynamicTest_Stop_PortStarted_ChannelNotBound
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private Mock<IChannelDirectTcpip> _channelMock;
        private ForwardedPortDynamic _forwardedPort;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;
        private IPEndPoint _endpoint;
        private Socket _client;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
            if (_forwardedPort != null)
            {
                _forwardedPort.Dispose();
                _forwardedPort = null;
            }
        }

        protected void Arrange()
        {
            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();
            _endpoint = new IPEndPoint(IPAddress.Loopback, 8122);

            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelDirectTcpip>(MockBehavior.Strict);

            _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(15));
            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _sessionMock.Setup(p => p.CreateChannelDirectTcpip()).Returns(_channelMock.Object);
            _channelMock.Setup(p => p.Dispose());

            _forwardedPort = new ForwardedPortDynamic(_endpoint.Address.ToString(), (uint) _endpoint.Port);
            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;
            _forwardedPort.Start();

            _client = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = 500,
                    SendTimeout = 500,
                    SendBufferSize = 0
                };
            _client.Connect(_endpoint);

            // allow for client socket to establish connection
            Thread.Sleep(50);
        }

        protected void Act()
        {
            _forwardedPort.Stop();
        }

        [TestMethod]
        public void IsStartedShouldReturnFalse()
        {
            Assert.IsFalse(_forwardedPort.IsStarted);
        }

        [TestMethod]
        public void ForwardedPortShouldRefuseNewConnections()
        {
            using (var client = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    client.Connect(_endpoint);
                    Assert.Fail();
                }
                catch (SocketException ex)
                {
                    Assert.AreEqual(SocketError.ConnectionRefused, ex.SocketErrorCode);
                }
            }
        }

        [TestMethod]
        public void ExistingConnectionShouldBeClosed()
        {
            try
            {
                _client.Send(new byte[] { 0x0a }, 0, 1, SocketFlags.None);
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                Assert.AreEqual(SocketError.ConnectionReset, ex.SocketErrorCode);
            }
        }

        [TestMethod]
        public void ClosingShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _closingRegister.Count);
        }

        [TestMethod]
        public void ExceptionShouldNotHaveFired()
        {
            Assert.AreEqual(0, _exceptionRegister.Count);
        }

        [TestMethod]
        public void DisposeOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Dispose(), Times.Once);
        }
    }
}
