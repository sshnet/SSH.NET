using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Read_ReadMode_NoDataInReaderBufferAndReadLessBytesFromServerThanCountAndLessThanBufferSize : SftpFileStreamTestBase
    {
        private string _path;
        private SftpFileStream _target;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private int _actual;
        private byte[] _buffer;
        private byte[] _serverData;
        private int _serverDataLength;
        private int _numberOfBytesToRead;
        private byte[] _originalBuffer;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();
            _path = random.Next().ToString();
            _handle = GenerateRandom(5, random);
            _bufferSize = (uint)random.Next(1, 1000);
            _readBufferSize = 20;
            _writeBufferSize = 500;

            _numberOfBytesToRead = (int) _readBufferSize + 2; // greater than read buffer size
            _originalBuffer = GenerateRandom(_numberOfBytesToRead, random);
            _buffer = _originalBuffer.Copy();

            _serverDataLength = (int) _readBufferSize - 1; // less than read buffer size
            _serverData = GenerateRandom(_serverDataLength, random);

            Assert.IsTrue(_serverDataLength < _numberOfBytesToRead && _serverDataLength < _readBufferSize);
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                .Setup(p => p.RequestOpen(_path, Flags.Read, false))
                .Returns(_handle);
            SftpSessionMock.InSequence(MockSequence)
                .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                .Returns(_readBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                .Returns(_writeBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                .Setup(p => p.IsOpen)
                .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                .Setup(p => p.RequestRead(_handle, 0UL, _readBufferSize))
                .Returns(_serverData);
        }

        [TestCleanup]
        public void TearDown()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestClose(_handle));
        }

        protected override void Arrange()
        {
            base.Arrange();

            _target = new SftpFileStream(SftpSessionMock.Object,
                                         _path,
                                         FileMode.Open,
                                         FileAccess.Read,
                                         (int)_bufferSize);
        }

        protected override void Act()
        {
            _actual = _target.Read(_buffer, 0, _numberOfBytesToRead);
        }

        [TestMethod]
        public void ReadShouldHaveReturnedTheNumberOfBytesReturnedByTheReadFromTheServer()
        {
            Assert.AreEqual(_serverDataLength, _actual);
        }

        [TestMethod]
        public void ReadShouldHaveWrittenBytesToTheCallerSuppliedBufferAndRemainingBytesShouldRemainUntouched()
        {
            Assert.IsTrue(_serverData.IsEqualTo(_buffer.Take(_serverDataLength)));
            Assert.IsTrue(_originalBuffer.Take(_serverDataLength, _originalBuffer.Length - _serverDataLength).IsEqualTo(_buffer.Take(_serverDataLength, _buffer.Length - _serverDataLength)));
        }

        [TestMethod]
        public void PositionShouldReturnNumberOfBytesWrittenToCallerProvidedBuffer()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            Assert.AreEqual(_actual, _target.Position);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public void SubsequentReadShouldReadAgainFromCurrentPositionFromServerAndReturnZeroWhenServerReturnsZeroBytes()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                .Setup(p => p.RequestRead(_handle, (ulong) _actual, _readBufferSize))
                .Returns(Array<byte>.Empty);

            var buffer = _originalBuffer.Copy();
            var actual = _target.Read(buffer, 0, buffer.Length);

            Assert.AreEqual(0, actual);
            Assert.IsTrue(_originalBuffer.IsEqualTo(buffer));

            SftpSessionMock.Verify(p => p.RequestRead(_handle, (ulong)_actual, _readBufferSize), Times.Once);
            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public void SubsequentReadShouldReadAgainFromCurrentPositionFromServerAndNotUpdatePositionWhenServerReturnsZeroBytes()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                .Setup(p => p.RequestRead(_handle, (ulong)_actual, _readBufferSize))
                .Returns(Array<byte>.Empty);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            _target.Read(new byte[10], 0, 10);

            Assert.AreEqual(_actual, _target.Position);

            SftpSessionMock.Verify(p => p.RequestRead(_handle, (ulong)_actual, _readBufferSize), Times.Once);
            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(3));
        }
    }
}
