#if FEATURE_TAP
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_ReadAsync_ReadMode_NoDataInReaderBufferAndReadLessBytesFromServerThanCountAndLessThanBufferSize : SftpFileStreamAsyncTestBase
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
                .Setup(p => p.RequestOpenAsync(_path, Flags.Read, default))
                .ReturnsAsync(_handle);
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
                .Setup(p => p.RequestReadAsync(_handle, 0UL, _readBufferSize, default))
                .ReturnsAsync(_serverData);
        }

        [TestCleanup]
        public void TearDown()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestClose(_handle));
        }

        protected override async Task ArrangeAsync()
        {
            await base.ArrangeAsync();

            _target = await SftpFileStream.OpenAsync(SftpSessionMock.Object,
                                         _path,
                                         FileMode.Open,
                                         FileAccess.Read,
                                         (int)_bufferSize,
                                         default);
        }

        protected override async Task ActAsync()
        {
            _actual = await _target.ReadAsync(_buffer, 0, _numberOfBytesToRead, default);
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
        public async Task SubsequentReadShouldReadAgainFromCurrentPositionFromServerAndReturnZeroWhenServerReturnsZeroBytes()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                .Setup(p => p.RequestReadAsync(_handle, (ulong) _actual, _readBufferSize, default))
                .ReturnsAsync(Array<byte>.Empty);

            var buffer = _originalBuffer.Copy();
            var actual = await _target.ReadAsync(buffer, 0, buffer.Length);

            Assert.AreEqual(0, actual);
            Assert.IsTrue(_originalBuffer.IsEqualTo(buffer));

            SftpSessionMock.Verify(p => p.RequestReadAsync(_handle, (ulong)_actual, _readBufferSize, default), Times.Once);
            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public async Task SubsequentReadShouldReadAgainFromCurrentPositionFromServerAndNotUpdatePositionWhenServerReturnsZeroBytes()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                .Setup(p => p.RequestReadAsync(_handle, (ulong)_actual, _readBufferSize, default))
                .ReturnsAsync(Array<byte>.Empty);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            await _target.ReadAsync(new byte[10], 0, 10);

            Assert.AreEqual(_actual, _target.Position);

            SftpSessionMock.Verify(p => p.RequestReadAsync(_handle, (ulong)_actual, _readBufferSize, default), Times.Once);
            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(3));
        }
    }
}
#endif