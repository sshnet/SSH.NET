using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ShellStreamTest_Write_WriteBufferEmptyAndWriteMoreBytesThanBufferSize
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private Mock<IChannelSession> _channelSessionMock;
        private MockSequence _mockSequence;
        private string _terminalName;
        private uint _widthColumns;
        private uint _heightRows;
        private uint _widthPixels;
        private uint _heightPixels;
        private Dictionary<TerminalModes, uint> _terminalModes;
        private ShellStream _shellStream;
        private int _bufferSize;

        private byte[] _data;
        private int _offset;
        private int _count;

        private byte[] _expectedBytesSent1;
        private byte[] _expectedBytesSent2;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        private void SetupData()
        {
            var random = new Random();

            _terminalName = random.Next().ToString();
            _widthColumns = (uint)random.Next();
            _heightRows = (uint)random.Next();
            _widthPixels = (uint)random.Next();
            _heightPixels = (uint)random.Next();
            _terminalModes = new Dictionary<TerminalModes, uint>();
            _bufferSize = random.Next(100, 1000);

            _data = CryptoAbstraction.GenerateRandom(_bufferSize * 2 + 10);
            _offset = 0;
            _count = _data.Length;

            _expectedBytesSent1 = _data.Take(0, _bufferSize);
            _expectedBytesSent2 = _data.Take(_bufferSize, _bufferSize);
        }

        private void CreateMocks()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            _mockSequence = new MockSequence();

            _sessionMock.InSequence(_mockSequence)
                        .Setup(p => p.ConnectionInfo)
                        .Returns(_connectionInfoMock.Object);
            _connectionInfoMock.InSequence(_mockSequence)
                               .Setup(p => p.Encoding)
                               .Returns(new UTF8Encoding());
            _sessionMock.InSequence(_mockSequence)
                        .Setup(p => p.CreateChannelSession())
                        .Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(_mockSequence)
                               .Setup(p => p.Open());
            _channelSessionMock.InSequence(_mockSequence)
                               .Setup(p => p.SendPseudoTerminalRequest(_terminalName,
                                                                       _widthColumns,
                                                                       _heightRows,
                                                                       _widthPixels,
                                                                       _heightPixels,
                                                                       _terminalModes))
                               .Returns(true);
            _channelSessionMock.InSequence(_mockSequence)
                               .Setup(p => p.SendShellRequest())
                               .Returns(true);
            _channelSessionMock.InSequence(_mockSequence)
                               .Setup(p => p.SendData(_expectedBytesSent1));
            _channelSessionMock.InSequence(_mockSequence)
                               .Setup(p => p.SendData(_expectedBytesSent2));
        }

        private void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _shellStream = new ShellStream(_sessionMock.Object,
                                           _terminalName,
                                           _widthColumns,
                                           _heightRows,
                                           _widthPixels,
                                           _heightPixels,
                                           _terminalModes,
                                           _bufferSize);
        }

        private void Act()
        {
            _shellStream.Write(_data, _offset, _count);
        }

        [TestMethod]
        public void BufferShouldHaveBeenFlushedTwice()
        {
            _channelSessionMock.Verify(p => p.SendData(_expectedBytesSent1), Times.Once);
            _channelSessionMock.Verify(p => p.SendData(_expectedBytesSent2), Times.Once);
        }

        [TestMethod]
        public void FlushShouldSendRemaningBytesToServer()
        {
            var expectedBytesSent = _data.Take(_bufferSize * 2, _data.Length - _bufferSize * 2);

            _channelSessionMock.InSequence(_mockSequence)
                               .Setup(p => p.SendData(expectedBytesSent));

            _shellStream.Flush();

            _channelSessionMock.Verify(p => p.SendData(expectedBytesSent), Times.Once);
        }
    }
}