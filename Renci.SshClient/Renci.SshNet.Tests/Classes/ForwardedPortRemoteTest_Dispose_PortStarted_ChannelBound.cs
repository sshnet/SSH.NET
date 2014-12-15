using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortRemoteTest_Dispose_PortStarted_ChannelBound
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private Mock<IChannelForwardedTcpip> _channelMock;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;
        private IPEndPoint _bindEndpoint;
        private IPEndPoint _remoteEndpoint;
        private TimeSpan _expectedElapsedTime;
        private TimeSpan _elapsedTimeOfStop;
        private uint _remoteChannelNumberWhileClosing;
        private uint _remoteWindowSizeWhileClosing;
        private uint _remotePacketSizeWhileClosing;
        private uint _remoteChannelNumberStarted;
        private uint _remoteWindowSizeStarted;
        private uint _remotePacketSizeStarted;
        private string _originatorAddress;
        private uint _originatorPort;

        protected ForwardedPortRemote ForwardedPort { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            Arrange();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Act();

            stopwatch.Stop();
            _elapsedTimeOfStop = stopwatch.Elapsed;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (ForwardedPort != null)
            {
                ForwardedPort.Dispose();
                ForwardedPort = null;
            }
        }

        protected void Arrange()
        {
            var random = new Random();
            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();
            _bindEndpoint = new IPEndPoint(IPAddress.Any, random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"), random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));
            _expectedElapsedTime = TimeSpan.FromMilliseconds(random.Next(100, 500));
            ForwardedPort = new ForwardedPortRemote(_bindEndpoint.Address, (uint)_bindEndpoint.Port, _remoteEndpoint.Address, (uint)_remoteEndpoint.Port);
            _remoteChannelNumberWhileClosing = (uint) random.Next(0, 1000);
            _remoteWindowSizeWhileClosing = (uint) random.Next(0, int.MaxValue);
            _remotePacketSizeWhileClosing = (uint) random.Next(0, int.MaxValue);
            _remoteChannelNumberStarted = (uint)random.Next(0, 1000);
            _remoteWindowSizeStarted = (uint)random.Next(0, int.MaxValue);
            _remotePacketSizeStarted = (uint)random.Next(0, int.MaxValue);
            _originatorAddress = random.Next().ToString(CultureInfo.InvariantCulture);
            _originatorPort = (uint)random.Next(0, int.MaxValue);

            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelForwardedTcpip>(MockBehavior.Strict);

            _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(15));
            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _sessionMock.Setup(p => p.RegisterMessage("SSH_MSG_REQUEST_FAILURE"));
            _sessionMock.Setup(p => p.RegisterMessage("SSH_MSG_REQUEST_SUCCESS"));
            _sessionMock.Setup(p => p.RegisterMessage("SSH_MSG_CHANNEL_OPEN"));
            _sessionMock.Setup(
                p =>
                    p.SendMessage(
                        It.Is<GlobalRequestMessage>(
                            g =>
                                g.RequestName == GlobalRequestName.TcpIpForward &&
                                g.AddressToBind == ForwardedPort.BoundHost &&
                                g.PortToBind == ForwardedPort.BoundPort)))
                .Callback(
                    () =>
                        _sessionMock.Raise(s => s.RequestSuccessReceived += null,
                            new MessageEventArgs<RequestSuccessMessage>(new RequestSuccessMessage())));
            _sessionMock.Setup(p => p.WaitOnHandle(It.IsAny<WaitHandle>()));
            _sessionMock.Setup(p => p.SendMessage(It.Is<ChannelOpenFailureMessage>(c => c.LocalChannelNumber == _remoteChannelNumberWhileClosing && c.ReasonCode == ChannelOpenFailureMessage.AdministrativelyProhibited && c.Description == string.Empty && c.Language == "en")));
            _sessionMock.Setup(p => p.CreateChannelForwardedTcpip(_remoteChannelNumberStarted, _remoteWindowSizeStarted, _remotePacketSizeStarted)).Returns(_channelMock.Object);
            _channelMock.Setup(
                p =>
                    p.Bind(
                        It.Is<IPEndPoint>(
                            ep => ep.Address.Equals(_remoteEndpoint.Address) && ep.Port == _remoteEndpoint.Port),
                        ForwardedPort)).Callback(() => Thread.Sleep(_expectedElapsedTime));
            _channelMock.Setup(p => p.Close());
            _channelMock.Setup(p => p.Dispose());
            _sessionMock.Setup(
                p =>
                    p.SendMessage(
                        It.Is<GlobalRequestMessage>(
                            g =>
                                g.RequestName == GlobalRequestName.CancelTcpIpForward &&
                                g.AddressToBind == ForwardedPort.BoundHost && g.PortToBind == ForwardedPort.BoundPort)));
            _sessionMock.Setup(p => p.MessageListenerCompleted).Returns(new ManualResetEvent(true));

            ForwardedPort.Closing += (sender, args) =>
                {
                    _closingRegister.Add(args);
                    _sessionMock.Raise(p => p.ChannelOpenReceived += null, new MessageEventArgs<ChannelOpenMessage>(new ChannelOpenMessage(_remoteChannelNumberWhileClosing, _remoteWindowSizeWhileClosing, _remotePacketSizeWhileClosing, new ForwardedTcpipChannelInfo(ForwardedPort.BoundHost, ForwardedPort.BoundPort, _originatorAddress, _originatorPort))));
                };
            ForwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            ForwardedPort.Session = _sessionMock.Object;
            ForwardedPort.Start();

            _sessionMock.Raise(p => p.ChannelOpenReceived += null, new MessageEventArgs<ChannelOpenMessage>(new ChannelOpenMessage(_remoteChannelNumberStarted, _remoteWindowSizeStarted, _remotePacketSizeStarted, new ForwardedTcpipChannelInfo(ForwardedPort.BoundHost, ForwardedPort.BoundPort, _originatorAddress, _originatorPort))));
        }

        protected virtual void Act()
        {
            ForwardedPort.Dispose();
        }

        [TestMethod]
        public void StopShouldBlockUntilBoundChannelHasClosed()
        {
            Assert.IsTrue(_elapsedTimeOfStop >= _expectedElapsedTime, string.Format("Expected {0} or greater but was {1}.", _expectedElapsedTime.TotalMilliseconds, _elapsedTimeOfStop.TotalMilliseconds));
            Assert.IsTrue(_elapsedTimeOfStop < _expectedElapsedTime.Add(TimeSpan.FromMilliseconds(200)));
        }

        [TestMethod]
        public void IsStartedShouldReturnFalse()
        {
            Assert.IsFalse(ForwardedPort.IsStarted);
        }

        [TestMethod]
        public void ForwardedPortShouldRejectChannelOpenMessagesThatAreReceivedWhileTheSuccessMessageForTheCancelOfTheForwardedPortIsNotReceived()
        {
            _sessionMock.Verify(p => p.SendMessage(new ChannelOpenFailureMessage(_remoteChannelNumberWhileClosing, string.Empty,
                        ChannelOpenFailureMessage.AdministrativelyProhibited)), Times.Never);
        }

        [TestMethod]
        public void ForwardedPortShouldIgnoreChannelOpenMessagesWhenDisposed()
        {
            var channelNumberDisposed = (uint)new Random().Next(1001, int.MaxValue);
            var initialWindowSizeDisposed = (uint)new Random().Next(0, int.MaxValue);
            var maximumPacketSizeDisposed = (uint)new Random().Next(0, int.MaxValue);
            var originatorAddressDisposed = new Random().Next().ToString(CultureInfo.InvariantCulture);
            var originatorPortDisposed = (uint)new Random().Next(0, int.MaxValue);
            var channelMock = new Mock<IChannelForwardedTcpip>(MockBehavior.Strict);

            _sessionMock.Setup(
                p =>
                    p.CreateChannelForwardedTcpip(channelNumberDisposed, initialWindowSizeDisposed, maximumPacketSizeDisposed)).Returns(channelMock.Object);
            _sessionMock.Setup(
                p =>
                    p.SendMessage(new ChannelOpenFailureMessage(channelNumberDisposed, string.Empty,
                        ChannelOpenFailureMessage.AdministrativelyProhibited)));

            _sessionMock.Raise(p => p.ChannelOpenReceived += null,
                new MessageEventArgs<ChannelOpenMessage>(new ChannelOpenMessage(channelNumberDisposed,
                    initialWindowSizeDisposed, maximumPacketSizeDisposed,
                    new ForwardedTcpipChannelInfo(ForwardedPort.BoundHost, ForwardedPort.BoundPort,
                        originatorAddressDisposed, originatorPortDisposed))));

            _sessionMock.Verify(p => p.CreateChannelForwardedTcpip(channelNumberDisposed, initialWindowSizeDisposed, maximumPacketSizeDisposed), Times.Never);
            _sessionMock.Verify(p => p.SendMessage(It.Is<ChannelOpenFailureMessage>(c => c.LocalChannelNumber == channelNumberDisposed && c.ReasonCode == ChannelOpenFailureMessage.AdministrativelyProhibited && c.Description == string.Empty && c.Language == null)), Times.Never);
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
        public void BindOnChannelShouldBeInvokedOnceForChannelOpenedWhileStarted()
        {
            _channelMock.Verify(
                c =>
                    c.Bind(
                        It.Is<IPEndPoint>(
                            ep => ep.Address.Equals(_remoteEndpoint.Address) && ep.Port == _remoteEndpoint.Port),
                        ForwardedPort), Times.Once);
        }

        [TestMethod]
        public void CloseOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Close(), Times.Once);
        }

        [TestMethod]
        public void DisposeOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Dispose(), Times.Once);
        }
    }
}
