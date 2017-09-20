using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_Dispose_SessionIsConnectedAndChannelIsOpen_EofReceived
    {
        private Mock<ISession> _sessionMock;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private ChannelStub _channel;
        private List<ChannelEventArgs> _channelClosedRegister;
        private List<ChannelEventArgs> _channelEndOfDataRegister;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private ManualResetEvent _channelClosedReceived;
        private Thread _raiseChannelCloseReceivedThread;

        private void SetupData()
        {
            var random = new Random();

            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _localWindowSize = (uint) random.Next(0, int.MaxValue);
            _localPacketSize = (uint) random.Next(0, int.MaxValue);
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint) random.Next(0, int.MaxValue);
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelEndOfDataRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _channelClosedReceived = new ManualResetEvent(false);
            _raiseChannelCloseReceivedThread = null;
        }

        private void CreateMocks()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            var sequence = new MockSequence();

            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence).Setup(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber))).Returns(true);
            _sessionMock.InSequence(sequence).Setup(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()))
                .Callback<WaitHandle>(w =>
                {
                    _raiseChannelCloseReceivedThread = new Thread(() =>
                    {
                        Thread.Sleep(100);

                        // signal that the ChannelCloseMessage was received; we use this to verify whether we've actually
                        // waited on the EventWaitHandle to be set; this needs to be set before we raise the ChannelCloseReceived
                        // to make sure the waithandle is signaled when the Dispose method completes (or else the assert that
                        // checks whether the handle has been signaled, will sometimes fail)
                        _channelClosedReceived.Set();

                        // raise ChannelCloseReceived event to set waithandle for receiving SSH_MSG_CHANNEL_CLOSE message
                        // from server which is waited on after sending the SSH_MSG_CHANNEL_CLOSE message to the server
                        //
                        // this will cause a new invocation of Close() that will block until the Close() that was invoked
                        // as part of Dispose() has released the lock; as such, this thread cannot be joined until that
                        // lock is released
                        //
                        // we're mocking the wait on the ChannelCloseMessage, but we still want
                        // to get the channel in the state that it would have after actually receiving
                        // the ChannelCloseMessage
                        _sessionMock.Raise(s => s.ChannelCloseReceived += null, new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
                    });
                    _raiseChannelCloseReceivedThread.Start();
                    w.WaitOne();
                });
        }

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_channelClosedReceived != null)
            {
                _channelClosedReceived.Dispose();
                _channelClosedReceived = null;
            }

            if (_raiseChannelCloseReceivedThread != null && _raiseChannelCloseReceivedThread.IsAlive)
            {
                if (!_raiseChannelCloseReceivedThread.Join(1000))
                {
                    _raiseChannelCloseReceivedThread.Abort();
                }
            }
        }

        private void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _channel = new ChannelStub(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.EndOfData += (sender, args) => _channelEndOfDataRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.InitializeRemoteChannelInfo(_remoteChannelNumber, _remoteWindowSize, _remotePacketSize);
            _channel.SetIsOpen(true);

            _sessionMock.Raise(
                s => s.ChannelEofReceived += null,
                new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));
        }

        private void Act()
        {
            _channel.Dispose();
        }

        [TestMethod]
        public void IsOpenShouldReturnFalse()
        {
            Assert.IsFalse(_channel.IsOpen);
        }

        [TestMethod]
        public void TrySendMessageOnSessionShouldBeInvokedOnceForChannelCloseMessage()
        {
            _sessionMock.Verify(
                p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Once);
        }

        [TestMethod]
        public void TrySendMessageOnSessionShouldNeverBeInvokedForChannelEofMessage()
        {
            _sessionMock.Verify(
                p => p.TrySendMessage(It.Is<ChannelEofMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Never);
        }

        [TestMethod]
        public void WaitOnHandleOnSessionShouldBeInvokedOnce()
        {
            _sessionMock.Verify(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()), Times.Once);
        }

        [TestMethod]
        public void WaitOnHandleOnSessionShouldWaitForChannelCloseMessageToBeReceived()
        {
            Assert.IsTrue(_channelClosedReceived.WaitOne(0));
        }

        [TestMethod]
        public void ClosedEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelClosedRegister.Count);
            Assert.AreEqual(_localChannelNumber, _channelClosedRegister[0].ChannelNumber);
        }

        [TestMethod]
        public void EndOfDataEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelEndOfDataRegister.Count);
            Assert.AreEqual(_localChannelNumber, _channelEndOfDataRegister[0].ChannelNumber);
        }

        [TestMethod]
        public void ExceptionShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelExceptionRegister.Count);
        }

        [TestMethod]
        public void ThreadThatRaisedChannelCloseReceivedShouldComplete()
        {
            _raiseChannelCloseReceivedThread.Join();
        }
    }
}
