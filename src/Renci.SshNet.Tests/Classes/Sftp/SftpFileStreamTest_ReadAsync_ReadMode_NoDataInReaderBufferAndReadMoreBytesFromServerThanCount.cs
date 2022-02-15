#if FEATURE_TAP
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;
using Renci.SshNet.Common;
using System.Threading.Tasks;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_ReadAsync_ReadMode_NoDataInReaderBufferAndReadMoreBytesFromServerThanCount : SftpFileStreamAsyncTestBase
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
        private int _numberOfBytesToWriteToReadBuffer;
        private int _numberOfBytesToRead;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();
            _path = random.Next().ToString();
            _handle = GenerateRandom(5, random);
            _bufferSize = (uint) random.Next(1, 1000);
            _readBufferSize = 20;
            _writeBufferSize = 500;

            _numberOfBytesToRead = 20;
            _buffer = new byte[_numberOfBytesToRead];
            _numberOfBytesToWriteToReadBuffer = 10; // should be less than _readBufferSize
            _serverData = GenerateRandom(_numberOfBytesToRead + _numberOfBytesToWriteToReadBuffer, random);
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
        public void ReadShouldHaveReturnedTheNumberOfBytesWrittenToCallerSuppliedBuffer()
        {
            Assert.AreEqual(_numberOfBytesToRead, _actual);
        }

        [TestMethod]
        public void ReadShouldHaveWrittenBytesToTheCallerSuppliedBuffer()
        {
            Assert.IsTrue(_serverData.Take(_actual).IsEqualTo(_buffer));
        }

        [TestMethod]
        public void PositionShouldReturnNumberOfBytesWrittenToCallerProvidedBuffer()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            Assert.AreEqual(_actual, _target.Position);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public async Task SubsequentReadShouldReturnAllRemaningBytesFromReadBufferWhenCountIsEqualToNumberOfRemainingBytes()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            var buffer = new byte[_numberOfBytesToWriteToReadBuffer];

            var actual = await _target.ReadAsync(buffer, 0, _numberOfBytesToWriteToReadBuffer, default);

            Assert.AreEqual(_numberOfBytesToWriteToReadBuffer, actual);
            Assert.IsTrue(_serverData.Take(_numberOfBytesToRead, _numberOfBytesToWriteToReadBuffer).IsEqualTo(buffer));

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public async Task SubsequentReadShouldReturnAllRemaningBytesFromReadBufferAndReadAgainWhenCountIsGreaterThanNumberOfRemainingBytesAndNewReadReturnsZeroBytes()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.RequestReadAsync(_handle, (ulong)(_serverData.Length), _readBufferSize, default)).ReturnsAsync(Array<byte>.Empty);

            var buffer = new byte[_numberOfBytesToWriteToReadBuffer + 1];

            var actual = await _target.ReadAsync(buffer, 0, buffer.Length);

            Assert.AreEqual(_numberOfBytesToWriteToReadBuffer, actual);
            Assert.IsTrue(_serverData.Take(_numberOfBytesToRead, _numberOfBytesToWriteToReadBuffer).IsEqualTo(buffer.Take(_numberOfBytesToWriteToReadBuffer)));
            Assert.AreEqual(0, buffer[_numberOfBytesToWriteToReadBuffer]);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
            SftpSessionMock.Verify(p => p.RequestReadAsync(_handle, (ulong)(_serverData.Length), _readBufferSize, default));
        }
    }
}
#endif