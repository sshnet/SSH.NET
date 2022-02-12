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
    public class ChannelTest_Dispose_SessionIsConnectedAndChannelIsOpen_EofReceived : ChannelTestBase
    {
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private TimeSpan _channelCloseTimeout;
        private ChannelStub _channel;
        private List<ChannelEventArgs> _channelClosedRegister;
        private List<ChannelEventArgs> _channelEndOfDataRegister;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private ManualResetEvent _channelClosedReceived;
        private ManualResetEvent _channelClosedEventHandlerCompleted;
        private Thread _raiseChannelCloseReceivedThread;

        protected override void SetupData()
        {
            var random = new Random();

            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _localWindowSize = (uint) random.Next(0, int.MaxValue);
            _localPacketSize = (uint) random.Next(0, int.MaxValue);
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint) random.Next(0, int.MaxValue);
            _channelCloseTimeout = TimeSpan.FromSeconds(random.Next(10, 20));
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelEndOfDataRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _channelClosedReceived = new ManualResetEvent(false);
            _channelClosedEventHandlerCompleted = new ManualResetEvent(false);
            _raiseChannelCloseReceivedThread = null;
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            SessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            SessionMock.InSequence(sequence).Setup(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber))).Returns(true);
            SessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
            ConnectionInfoMock.InSequence(sequence).Setup(p => p.ChannelCloseTimeout).Returns(_channelCloseTimeout);
            SessionMock.InSequence(sequence)
                       .Setup(p => p.TryWait(It.IsAny<EventWaitHandle>(), _channelCloseTimeout))
                       .Callback<WaitHandle, TimeSpan>((waitHandle, channelCloseTimeout) =>
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
                               SessionMock.Raise(s => s.ChannelCloseReceived += null, new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
                           });
                           _raiseChannelCloseReceivedThread.Start();
                           waitHandle.WaitOne();
                       })
                       .Returns(WaitResult.Success);
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

            if (_channelClosedEventHandlerCompleted != null)
            {
                _channelClosedEventHandlerCompleted.Dispose();
                _channelClosedEventHandlerCompleted = null;
            }
        }

        protected override void Arrange()
        {
            base.Arrange();

            _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) =>
                {
                    _channelClosedRegister.Add(args);
                    Thread.Sleep(50);
                    _channelClosedEventHandlerCompleted.Set();
                };
            _channel.EndOfData += (sender, args) => _channelEndOfDataRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.InitializeRemoteChannelInfo(_remoteChannelNumber, _remoteWindowSize, _remotePacketSize);
            _channel.SetIsOpen(true);

            SessionMock.Raise(
                s => s.ChannelEofReceived += null,
                new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));
        }

        protected override void Act()
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
            SessionMock.Verify(
                p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Once);
        }

        [TestMethod]
        public void TrySendMessageOnSessionShouldNeverBeInvokedForChannelEofMessage()
        {
            SessionMock.Verify(
                p => p.TrySendMessage(It.Is<ChannelEofMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Never);
        }

        [TestMethod]
        public void TryWaitOnSessionShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.TryWait(It.IsAny<EventWaitHandle>(), _channelCloseTimeout), Times.Once);
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
        public void EndOfDataEventShouldNotHaveFired()
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
