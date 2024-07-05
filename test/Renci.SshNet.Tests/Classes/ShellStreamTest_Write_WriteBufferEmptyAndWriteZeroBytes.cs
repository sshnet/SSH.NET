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
    public class ShellStreamTest_Write_WriteBufferEmptyAndWriteZeroBytes
    {
        private Mock<ISession> _sessionMock;
        private Mock<ISshConnectionInfo> _connectionInfoMock;
        private Mock<IChannelSession> _channelSessionMock;
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
        private MockSequence _mockSequence;

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

            _data = new byte[0];
            _offset = 0;
            _count = _data.Length;
        }

        private void CreateMocks()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<ISshConnectionInfo>(MockBehavior.Strict);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            _mockSequence = new MockSequence();

            _ = _sessionMock.InSequence(_mockSequence)
                            .Setup(p => p.ConnectionInfo)
                            .Returns(_connectionInfoMock.Object);
            _ = _connectionInfoMock.InSequence(_mockSequence)
                                   .Setup(p => p.Encoding)
                                   .Returns(new UTF8Encoding());
            _ = _sessionMock.InSequence(_mockSequence)
                            .Setup(p => p.CreateChannelSession())
                            .Returns(_channelSessionMock.Object);
            _ = _channelSessionMock.InSequence(_mockSequence)
                                   .Setup(p => p.Open());
            _ = _channelSessionMock.InSequence(_mockSequence)
                                    .Setup(p => p.SendPseudoTerminalRequest(_terminalName,
                                                                            _widthColumns,
                                                                            _heightRows,
                                                                            _widthPixels,
                                                                            _heightPixels,
                                                                            _terminalModes))
                                    .Returns(true);
            _ = _channelSessionMock.InSequence(_mockSequence)
                                   .Setup(p => p.SendShellRequest())
                                   .Returns(true);
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
        public void NoDataShouldBeSentToServer()
        {
            _channelSessionMock.Verify(p => p.SendData(It.IsAny<byte[]>()), Times.Never);
        }

        [TestMethod]
        public void FlushShouldSendNoBytesToServer()
        {
            _shellStream.Flush();

            _channelSessionMock.Verify(p => p.SendData(It.IsAny<byte[]>()), Times.Never);
        }
    }
}
