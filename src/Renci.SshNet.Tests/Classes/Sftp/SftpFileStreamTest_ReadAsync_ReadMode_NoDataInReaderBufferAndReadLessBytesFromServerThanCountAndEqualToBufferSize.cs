#if FEATURE_TAP
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_ReadAsync_ReadMode_NoDataInReaderBufferAndReadLessBytesFromServerThanCountAndEqualToBufferSize : SftpFileStreamAsyncTestBase
    {
        private string _path;
        private SftpFileStream _target;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private int _actual;
        private byte[] _buffer;
        private byte[] _serverData1;
        private byte[] _serverData2;
        private int _serverData1Length;
        private int _serverData2Length;
        private int _numberOfBytesToRead;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();
            _path = random.Next().ToString();
            _handle = GenerateRandom(5, random);
            _bufferSize = (uint)random.Next(1, 1000);
            _readBufferSize = 20;
            _writeBufferSize = 500;

            _numberOfBytesToRead = (int) _readBufferSize + 5; // greather than read buffer size
            _buffer = new byte[_numberOfBytesToRead];
            _serverData1Length = (int) _readBufferSize; // equal to read buffer size
            _serverData1 = GenerateRandom(_serverData1Length, random);
            _serverData2Length = (int) _readBufferSize; // equal to read buffer size
            _serverData2 = GenerateRandom(_serverData2Length, random);

            Assert.IsTrue(_serverData1Length < _numberOfBytesToRead && _serverData1Length == _readBufferSize);
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
                .ReturnsAsync(_serverData1);
            SftpSessionMock.InSequence(MockSequence)
                .Setup(p => p.RequestReadAsync(_handle, (ulong)_serverData1.Length, _readBufferSize, default))
                .ReturnsAsync(_serverData2);
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
        public void ReadShouldHaveReturnedTheNumberOfBytesRequested()
        {
            Assert.AreEqual(_numberOfBytesToRead, _actual);
        }

        [TestMethod]
        public void ReadShouldHaveWrittenBytesToTheCallerSuppliedBuffer()
        {
            Assert.IsTrue(_serverData1.IsEqualTo(_buffer.Take(_serverData1Length)));

            var bytesWrittenFromSecondRead = _numberOfBytesToRead - _serverData1Length;
            Assert.IsTrue(_serverData2.Take(bytesWrittenFromSecondRead).IsEqualTo(_buffer.Take(_serverData1Length, bytesWrittenFromSecondRead)));
        }

        [TestMethod]
        public void PositionShouldReturnNumberOfBytesWrittenToCallerProvidedBuffer()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            Assert.AreEqual(_actual, _target.Position);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public async Task ReadShouldReturnAllRemaningBytesFromReadBufferWhenCountIsEqualToNumberOfRemainingBytes()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            var numberOfBytesRemainingInReadBuffer = _serverData1Length + _serverData2Length - _numberOfBytesToRead;

            _buffer = new byte[numberOfBytesRemainingInReadBuffer];

            var actual = await _target.ReadAsync(_buffer, 0, _buffer.Length);

            Assert.AreEqual(_buffer.Length, actual);
            Assert.IsTrue(_serverData2.Take(_numberOfBytesToRead - _serverData1Length, _buffer.Length).IsEqualTo(_buffer));

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public async Task ReadShouldReturnAllRemaningBytesFromReadBufferAndReadAgainWhenCountIsGreaterThanNumberOfRemainingBytesAndNewReadReturnsZeroBytes()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.RequestReadAsync(_handle, (ulong)(_serverData1Length + _serverData2Length), _readBufferSize, default)).ReturnsAsync(Array<byte>.Empty);

            var numberOfBytesRemainingInReadBuffer = _serverData1Length + _serverData2Length - _numberOfBytesToRead;

            _buffer = new byte[numberOfBytesRemainingInReadBuffer + 1];

            var actual = await _target.ReadAsync(_buffer, 0, _buffer.Length);

            Assert.AreEqual(numberOfBytesRemainingInReadBuffer, actual);
            Assert.IsTrue(_serverData2.Take(_numberOfBytesToRead - _serverData1Length, numberOfBytesRemainingInReadBuffer).IsEqualTo(_buffer.Take(numberOfBytesRemainingInReadBuffer)));
            Assert.AreEqual(0, _buffer[numberOfBytesRemainingInReadBuffer]);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
            SftpSessionMock.Verify(p => p.RequestReadAsync(_handle, (ulong)(_serverData1Length + _serverData2Length), _readBufferSize, default));
        }
    }
}
#endif