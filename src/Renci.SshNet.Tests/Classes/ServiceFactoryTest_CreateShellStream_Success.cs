using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ServiceFactoryTest_CreateShellStream_Success
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private Mock<IChannelSession> _channelSessionMock;
        private ServiceFactory _serviceFactory;
        private string _terminalName;
        private uint _columns;
        private uint _rows;
        private uint _width;
        private uint _height;
        private IDictionary<TerminalModes, uint> _terminalModeValues;
        private int _bufferSize;
        private ShellStream _shellStream;

        private void SetupData()
        {
            var random = new Random();

            _terminalName = random.Next().ToString();
            _columns = (uint) random.Next();
            _rows = (uint) random.Next();
            _width = (uint) random.Next();
            _height = (uint) random.Next();
            _terminalModeValues = new Dictionary<TerminalModes, uint>();
            _bufferSize = random.Next();
        }

        private void CreateMocks()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            var sequence = new MockSequence();

            _sessionMock.InSequence(sequence)
                        .Setup(p => p.ConnectionInfo)
                        .Returns(_connectionInfoMock.Object);
            _connectionInfoMock.InSequence(sequence)
                               .Setup(p => p.Encoding)
                               .Returns(new UTF8Encoding());
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.CreateChannelSession())
                        .Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.Open());
            _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.SendPseudoTerminalRequest(_terminalName,
                                                                       _columns,
                                                                       _rows,
                                                                       _width,
                                                                       _height,
                                                                       _terminalModeValues))
                               .Returns(true);
            _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.SendShellRequest())
                               .Returns(true);
        }

        private void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _serviceFactory = new ServiceFactory();
        }

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        private void Act()
        {
            _shellStream = _serviceFactory.CreateShellStream(_sessionMock.Object,
                                                             _terminalName,
                                                             _columns,
                                                             _rows,
                                                             _width,
                                                             _height,
                                                             _terminalModeValues,
                                                             _bufferSize);
        }

        [TestMethod]
        public void CreateShellStreamShouldNotReturnNull()
        {
            Assert.IsNotNull(_shellStream);
        }

        [TestMethod]
        public void BufferSizeOfShellStreamShouldBeValuePassedToCreateShellStream()
        {
            Assert.AreEqual(_bufferSize, _shellStream.BufferSize);
        }

        [TestMethod]
        public void SendPseudoTerminalRequestShouldHaveBeenInvokedOnce()
        {
            _channelSessionMock.Verify(p => p.SendPseudoTerminalRequest(_terminalName,
                                                                        _columns,
                                                                        _rows,
                                                                        _width,
                                                                        _height,
                                                                        _terminalModeValues),
                                       Times.Once);
        }

        [TestMethod]
        public void SendShellRequestShouldHaveBeenInvokedOnce()
        {
            _channelSessionMock.Verify(p => p.SendShellRequest(), Times.Once);
        }
    }
}