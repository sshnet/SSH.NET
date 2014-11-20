using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelForwardedTcpipTest_Close_SessionIsConnectedAndChannelIsOpen
    {
        private Mock<ISession> _sessionMock;
        private Mock<IForwardedPort> _forwardedPortMock;
        private ChannelForwardedTcpip _channel;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private uint _remoteChannelNumber;
        private IPEndPoint _remoteEndpoint;
        private AsyncSocketListener _remoteListener;
        private EventWaitHandle _channelBindFinishedWaitHandle;
        private Exception _channelException;
        private IList<Socket> _connectedRegister;
        private IList<Socket> _disconnectedRegister;
        private Thread _channelThread;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (_remoteListener != null)
            {
                _remoteListener.Stop();
                _remoteListener = null;
            }

            if (_channelThread != null)
            {
                if (_channelThread.IsAlive)
                    _channelThread.Abort();
            }
        }

        private void Arrange()
        {
            var random = new Random();
            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _localWindowSize = (uint) random.Next(2000, 3000);
            _localPacketSize = (uint) random.Next(1000, 2000);
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint) random.Next(100, 200);
            _channelBindFinishedWaitHandle = new ManualResetEvent(false);
            _channelException = null;
            _connectedRegister = new List<Socket>();
            _disconnectedRegister = new List<Socket>();

            _remoteEndpoint = new IPEndPoint(IPAddress.Loopback, 8122);

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _forwardedPortMock = new Mock<IForwardedPort>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence).Setup(
                p => p.SendMessage(
                    It.Is<ChannelOpenConfirmationMessage>(
                        m => m.LocalChannelNumber == _remoteChannelNumber
                             &&
                             m.InitialWindowSize == _localWindowSize
                             &&
                             m.MaximumPacketSize == _localPacketSize
                             &&
                             m.RemoteChannelNumber == _localChannelNumber)
                    ));
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence)
                .Setup(
                    p => p.TrySendMessage(It.Is<ChannelEofMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)))
                .Returns(true);
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence)
                .Setup(
                    p => p.TrySendMessage(It.Is<ChannelCloseMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)))
                .Returns(true);
            _sessionMock.InSequence(sequence)
                .Setup(p => p.WaitOnHandle(It.IsNotNull<WaitHandle>()))
                .Callback<WaitHandle>(
                    w =>
                        {
                            _sessionMock.Raise(
                                s => s.ChannelCloseReceived += null,
                                new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
                            w.WaitOne();
                        });

            _remoteListener = new AsyncSocketListener(_remoteEndpoint);
            _remoteListener.Connected += socket => _connectedRegister.Add(socket);
            _remoteListener.Disconnected += socket => _disconnectedRegister.Add(socket);
            _remoteListener.Start();

            _channel = new ChannelForwardedTcpip(
                _sessionMock.Object,
                _localChannelNumber,
                _localWindowSize,
                _localPacketSize,
                _remoteChannelNumber,
                _remoteWindowSize,
                _remotePacketSize);

            _channelThread = new Thread(() =>
                {
                    try
                    {
                        _channel.Bind(_remoteEndpoint, _forwardedPortMock.Object);
                    }
                    catch (Exception ex)
                    {
                        _channelException = ex;
                    }
                    finally
                    {
                        _channelBindFinishedWaitHandle.Set();
                    }
                });
            _channelThread.Start();

            // give channel time to bind to remote endpoint
            Thread.Sleep(100);
        }

        private void Act()
        {
            _channel.Close();
        }

        [TestMethod]
        public void ChannelShouldShutdownSocketToRemoteListener()
        {
            Assert.AreEqual(1, _connectedRegister.Count);
            Assert.AreEqual(1, _disconnectedRegister.Count);
            Assert.AreSame(_connectedRegister[0], _disconnectedRegister[0]);
        }

        [TestMethod]
        public void BindShouldHaveFinishedWithoutException()
        {
            Assert.IsTrue(_channelBindFinishedWaitHandle.WaitOne(0));
            Assert.IsNull(_channelException, _channelException != null ? _channelException.ToString() : null);
        }

        [TestMethod]
        public void ChannelEofMessageShouldBeSentOnce()
        {
            _sessionMock.Verify(p => p.TrySendMessage(It.Is<ChannelEofMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)), Times.Once);
        }

        [TestMethod]
        public void ChannelCloseMessageShouldBeSentOnce()
        {
            _sessionMock.Verify(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)), Times.Once);
        }

        [TestMethod]
        public void IsOpenShouldReturnFalse()
        {
            Assert.IsFalse(_channel.IsOpen);
        }
    }
}