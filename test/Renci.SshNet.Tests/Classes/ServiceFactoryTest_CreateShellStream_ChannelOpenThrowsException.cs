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
    public class ServiceFactoryTest_CreateShellStream_ChannelOpenThrowsException
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
        private SshException _channelOpenException;
        private SshException _actualException;

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
            _channelOpenException = new SshException();

            _actualException = null;
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
                               .Setup(p => p.Open())
                               .Throws(_channelOpenException);
            _channelSessionMock.InSequence(sequence)
                               .Setup(p => p.Dispose());
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
            try
            {
                _serviceFactory.CreateShellStream(_sessionMock.Object,
                                                  _terminalName,
                                                  _columns,
                                                  _rows,
                                                  _width,
                                                  _height,
                                                  _terminalModeValues,
                                                  _bufferSize);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void CreateShellStreamShouldRethrowExceptionThrownByOpenOnChannelSession()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreSame(_channelOpenException, _actualException);
        }

        [TestMethod]
        public void DisposeOnChannelSessionShouldHaveBeenInvokedOnce()
        {
            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
        }
    }
}