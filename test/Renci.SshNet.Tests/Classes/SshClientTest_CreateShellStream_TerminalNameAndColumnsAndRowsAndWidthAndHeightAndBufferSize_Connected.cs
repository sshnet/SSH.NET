using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshClientTest_CreateShellStream_TerminalNameAndColumnsAndRowsAndWidthAndHeightAndBufferSize_Connected : BaseClientTestBase
    {
        private SshClient _sshClient;
        private ConnectionInfo _connectionInfo;
        private string _terminalName;
        private uint _widthColumns;
        private uint _heightRows;
        private uint _widthPixels;
        private uint _heightPixels;
        private int _bufferSize;
        private int _expectSize;
        private ShellStream _expected;
        private ShellStream _actual;

        protected override void SetupData()
        {
            var random = new Random();

            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));

            _terminalName = random.Next().ToString();
            _widthColumns = (uint)random.Next();
            _heightRows = (uint)random.Next();
            _widthPixels = (uint)random.Next();
            _heightPixels = (uint)random.Next();
            _bufferSize = random.Next(100, 1000);
            _expectSize = random.Next(100, _bufferSize);

            _expected = CreateShellStream();
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSocketFactory())
                                   .Returns(SocketFactoryMock.Object);
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                                   .Returns(SessionMock.Object);
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.Connect());
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateShellStream(SessionMock.Object,
                                                                   _terminalName,
                                                                   _widthColumns,
                                                                   _heightRows,
                                                                   _widthPixels,
                                                                   _heightPixels,
                                                                   null,
                                                                   _bufferSize,
                                                                   _expectSize))
                                   .Returns(_expected);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _sshClient = new SshClient(_connectionInfo, false, ServiceFactoryMock.Object);
            _sshClient.Connect();
        }

        protected override void Act()
        {
            _actual = _sshClient.CreateShellStream(_terminalName,
                                                   _widthColumns,
                                                   _heightRows,
                                                   _widthPixels,
                                                   _heightPixels,
                                                   _bufferSize,
                                                   _expectSize);
        }

        [TestMethod]
        public void CreateShellStreamOnServiceFactoryShouldBeInvokedOnce()
        {
            ServiceFactoryMock.Verify(p => p.CreateShellStream(SessionMock.Object,
                                                                _terminalName,
                                                                _widthColumns,
                                                                _heightRows,
                                                                _widthPixels,
                                                                _heightPixels,
                                                                null,
                                                                _bufferSize,
                                                                _expectSize),
                                       Times.Once);
        }

        [TestMethod]
        public void CreateShellStreamShouldReturnValueReturnedByCreateShellStreamOnServiceFactory()
        {
            Assert.IsNotNull(_actual);
            Assert.AreSame(_expected, _actual);
        }

        private ShellStream CreateShellStream()
        {
            var sessionMock = new Mock<ISession>(MockBehavior.Loose);
            var channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);

            _ = sessionMock.Setup(p => p.ConnectionInfo)
                           .Returns(new ConnectionInfo("A", "B", new PasswordAuthenticationMethod("A", "B")));
            _ = sessionMock.Setup(p => p.CreateChannelSession())
                           .Returns(channelSessionMock.Object);
            _ = channelSessionMock.Setup(p => p.Open());
            _ = channelSessionMock.Setup(p => p.SendPseudoTerminalRequest(_terminalName,
                                                                      _widthColumns,
                                                                      _heightRows,
                                                                      _widthPixels,
                                                                      _heightPixels,
                                                                      null))
                                  .Returns(true);
            _ = channelSessionMock.Setup(p => p.SendShellRequest())
                                  .Returns(true);

            return new ShellStream(sessionMock.Object,
                                   _terminalName,
                                   _widthColumns,
                                   _heightRows,
                                   _widthPixels,
                                   _heightPixels,
                                   null,
                                   1,
                                   1);
        }
    }
}
