using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_ReadByte_ReadMode_NoDataInWriteBufferAndNoDataInReadBuffer_Eof
    {
        private Mock<ISftpSession> _sftpSessionMock;
        private string _path;
        private SftpFileStream _sftpFileStream;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private int _actual;
        private MockSequence _sequence;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        public void TearDown()
        {
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.RequestClose(_handle));
        }

        protected void Arrange()
        {
            var random = new Random();
            _path = random.Next().ToString();
            _handle = new[] {(byte) random.Next(byte.MinValue, byte.MaxValue)};
            _bufferSize = (uint) random.Next(1, 1000);
            _readBufferSize = (uint) random.Next(0, 1000);
            _writeBufferSize = (uint) random.Next(0, 1000);

            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);

            _sequence = new MockSequence();

            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Truncate, true))
                .Returns(_handle);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                .Returns(_readBufferSize);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                .Returns(_writeBufferSize);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.IsOpen)
                .Returns(true);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.RequestRead(_handle, 0UL, _readBufferSize))
                .Returns(new byte[0]);

            _sftpFileStream = new SftpFileStream(_sftpSessionMock.Object, _path, FileMode.Create, FileAccess.Read, (int)_bufferSize);
        }

        protected void Act()
        {
            _actual = _sftpFileStream.ReadByte();
        }

        [TestMethod]
        public void ReadByteShouldReturnMinusOne()
        {
            Assert.AreEqual(-1, _actual);
        }

        [TestMethod]
        public void PositionShouldReturnZero()
        {
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.IsOpen)
                            .Returns(true);

            Assert.AreEqual(0L, _sftpFileStream.Position);
        }
    }
}
