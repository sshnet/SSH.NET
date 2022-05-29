#if FEATURE_TAP
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_WriteAsync_SessionOpen_CountGreatherThanTwoTimesTheWriteBufferSize : SftpFileStreamAsyncTestBase
    {
        private SftpFileStream _target;
        private string _path;
        private byte[] _handle;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private byte[] _data;
        private int _count;
        private int _offset;
        private Random _random;
        private uint _expectedWrittenByteCount;
        private int _expectedBufferedByteCount;
        private byte[] _expectedBufferedBytes;
        private CancellationToken _cancellationToken;

        protected override void SetupData()
        {
            base.SetupData();

            _random = new Random();
            _path = _random.Next().ToString(CultureInfo.InvariantCulture);
            _handle = GenerateRandom(5, _random);
            _bufferSize = (uint)_random.Next(1, 1000);
            _readBufferSize = (uint) _random.Next(0, 1000);
            _writeBufferSize = (uint) _random.Next(500, 1000);
            _data = new byte[(_writeBufferSize * 2) + 15];
            _random.NextBytes(_data);
            _offset = _random.Next(1, 5);
            // to get multiple SSH_FXP_WRITE messages (and verify the offset is updated correctly), we make sure
            // the number of bytes to write is at least two times the write buffer size; we write a few extra bytes to
            // ensure the buffer is not empty after the writes so we can verify whether Length, Dispose and Flush
            // flush the buffer
            _count = ((int) _writeBufferSize * 2) + _random.Next(1, 5);

            _expectedWrittenByteCount = (2 * _writeBufferSize);
            _expectedBufferedByteCount = (int)(_count - _expectedWrittenByteCount);
            _expectedBufferedBytes = _data.Take(_offset + (int)_expectedWrittenByteCount, _expectedBufferedByteCount);
            _cancellationToken = new CancellationToken();
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestOpenAsync(_path, Flags.Write | Flags.CreateNewOrOpen | Flags.Truncate, _cancellationToken))
                           .ReturnsAsync(_handle);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                           .Returns(_readBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                           .Returns(_writeBufferSize);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestWriteAsync(_handle, 0, _data, _offset, (int)_writeBufferSize, _cancellationToken))
                           .Returns(Task.CompletedTask);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestWriteAsync(_handle, _writeBufferSize, _data, _offset + (int)_writeBufferSize, (int)_writeBufferSize, _cancellationToken))
                           .Returns(Task.CompletedTask);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (SftpSessionMock != null)
            {
                // allow Dispose to complete successfully
                SftpSessionMock.InSequence(MockSequence)
                               .Setup(p => p.IsOpen)
                               .Returns(true);
                SftpSessionMock.InSequence(MockSequence)
                               .Setup(p => p.RequestWriteAsync(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, _cancellationToken))
                               .Returns(Task.CompletedTask);
                SftpSessionMock.InSequence(MockSequence)
                               .Setup(p => p.RequestClose(_handle));
            }
        }

        protected override async Task ArrangeAsync()
        {
            await base.ArrangeAsync();

            _target = await SftpFileStream.OpenAsync(SftpSessionMock.Object, _path, FileMode.Create, FileAccess.Write, (int) _bufferSize, _cancellationToken);
        }

        protected override Task ActAsync()
        {
            return _target.WriteAsync(_data, _offset, _count);
        }

        [TestMethod]
        public void RequestWriteOnSftpSessionShouldBeInvokedTwice()
        {
            SftpSessionMock.Verify(p => p.RequestWriteAsync(_handle, 0, _data, _offset, (int)_writeBufferSize, _cancellationToken), Times.Once);
            SftpSessionMock.Verify(p => p.RequestWriteAsync(_handle, _writeBufferSize, _data, _offset + (int)_writeBufferSize, (int)_writeBufferSize, _cancellationToken), Times.Once);
        }

        [TestMethod]
        public void PositionShouldBeNumberOfBytesWrittenToFileAndNUmberOfBytesInBuffer()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            Assert.AreEqual(_count, _target.Position);
        }

        [TestMethod]
        public async Task FlushShouldFlushBuffer()
        {
            byte[] actualFlushedData = null;

            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestWriteAsync(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, _cancellationToken))
                           .Callback<byte[], ulong, byte[], int, int, CancellationToken>((handle, serverFileOffset, data, offset, length, ct) => actualFlushedData = data.Take(offset, length))
                           .Returns(Task.CompletedTask);

            await _target.FlushAsync();

            Assert.IsTrue(actualFlushedData.IsEqualTo(_expectedBufferedBytes));

            SftpSessionMock.Verify(p => p.RequestWriteAsync(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, _cancellationToken), Times.Once);
        }
    }
}
#endif