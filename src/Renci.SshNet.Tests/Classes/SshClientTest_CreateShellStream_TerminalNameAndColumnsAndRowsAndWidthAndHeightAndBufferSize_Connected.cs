using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshClientTest_CreateShellStream_TerminalNameAndColumnsAndRowsAndWidthAndHeightAndBufferSize_Connected
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private SshClient _sshClient;
        private ConnectionInfo _connectionInfo;
        private string _terminalName;
        private uint _widthColumns;
        private uint _heightRows;
        private uint _widthPixels;
        private uint _heightPixels;
        private int _bufferSize;
        private ShellStream _expected;
        private ShellStream _actual;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        private void SetupData()
        {
            var random = new Random();

            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));

            _terminalName = random.Next().ToString();
            _widthColumns = (uint)random.Next();
            _heightRows = (uint)random.Next();
            _widthPixels = (uint)random.Next();
            _heightPixels = (uint)random.Next();
            _bufferSize = random.Next(100, 1000);

            _expected = CreateShellStream();
        }

        private void CreateMocks()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            var sequence = new MockSequence();

            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSession(_connectionInfo))
                               .Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.Connect());
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateShellStream(_sessionMock.Object,
                                                               _terminalName,
                                                               _widthColumns,
                                                               _heightRows,
                                                               _widthPixels,
                                                               _heightPixels,
                                                               null,
                                                               _bufferSize))
                               .Returns(_expected);
        }

        private void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _sshClient = new SshClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _sshClient.Connect();
        }

        protected void Act()
        {
            _actual = _sshClient.CreateShellStream(_terminalName,
                                                   _widthColumns,
                                                   _heightRows,
                                                   _widthPixels,
                                                   _heightPixels,
                                                   _bufferSize);
        }

        [TestMethod]
        public void CreateShellStreamOnServiceFactoryShouldBeInvokedOnce()
        {
            _serviceFactoryMock.Verify(p => p.CreateShellStream(_sessionMock.Object,
                                                                _terminalName,
                                                                _widthColumns,
                                                                _heightRows,
                                                                _widthPixels,
                                                                _heightPixels,
                                                                null,
                                                                _bufferSize),
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

            sessionMock.Setup(p => p.ConnectionInfo)
                       .Returns(new ConnectionInfo("A", "B", new PasswordAuthenticationMethod("A", "B")));
            sessionMock.Setup(p => p.CreateChannelSession())
                       .Returns(channelSessionMock.Object);
            channelSessionMock.Setup(p => p.Open());
            channelSessionMock.Setup(p => p.SendPseudoTerminalRequest(_terminalName,
                                                                      _widthColumns,
                                                                      _heightRows,
                                                                      _widthPixels,
                                                                      _heightPixels,
                                                                      null))
                              .Returns(true);
            channelSessionMock.Setup(p => p.SendShellRequest())
                              .Returns(true);

            return new ShellStream(sessionMock.Object,
                                   _terminalName,
                                   _widthColumns,
                                   _heightRows,
                                   _widthPixels,
                                   _heightPixels,
                                   null,
                                   1);
        }
    }
}
