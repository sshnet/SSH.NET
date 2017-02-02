using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Write_SessionOpen_CountGreatherThanTwoTimesTheWriteBufferSize
    {
        private Mock<ISftpSession> _sftpSessionMock;
        private string _path;
        private SftpFileStream _sftpFileStream;
        private byte[] _handle;
        private SftpFileAttributes _fileAttributes;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private byte[] _data;
        private int _count;
        private int _offset;
        private MockSequence _sequence;
        private Random _random;
        private uint _expectedWrittenByteCount;
        private int _expectedBufferedByteCount;
        private byte[] _expectedBufferedBytes;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_sftpSessionMock != null)
            {
                // allow Dispose to complete successfully
                _sftpSessionMock.InSequence(_sequence)
                    .Setup(p => p.IsOpen)
                    .Returns(true);
                _sftpSessionMock.InSequence(_sequence)
                    .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null));
                _sftpSessionMock.InSequence(_sequence)
                    .Setup(p => p.RequestClose(_handle));
            }
        }

        protected void Arrange()
        {
            _random = new Random();
            _path = _random.Next().ToString(CultureInfo.InvariantCulture);
            _handle = new[] {(byte) _random.Next(byte.MinValue, byte.MaxValue)};
            _fileAttributes = SftpFileAttributes.Empty;
            _bufferSize = (uint) _random.Next(1, 1000);
            _readBufferSize = (uint) _random.Next(0, 1000);
            _writeBufferSize = (uint) _random.Next(500, 1000);
            _data = new byte[(_writeBufferSize  * 2) + 15];
            _random.NextBytes(_data);
            _offset = _random.Next(1, 5);
            // to get multiple SSH_FXP_WRITE messages (and verify the offset is updated correctly), we make sure
            // the number of bytes to write is at least two times the write buffer size; we write a few extra bytes to
            // ensure the buffer is not empty after the writes so we can verify whether Length, Dispose and Flush
            // flush the buffer
            _count = ((int) _writeBufferSize*2) + _random.Next(1, 5);

            _expectedWrittenByteCount = (2 * _writeBufferSize);
            _expectedBufferedByteCount = (int)(_count - _expectedWrittenByteCount);
            _expectedBufferedBytes = _data.Take(_offset + (int)_expectedWrittenByteCount, _expectedBufferedByteCount);

            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);

            _sequence = new MockSequence();
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.RequestOpen(_path, Flags.Write | Flags.Truncate, true))
                .Returns(_handle);
            _sftpSessionMock.InSequence(_sequence).Setup(p => p.RequestFStat(_handle, false)).Returns(_fileAttributes);
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
                .Setup(p => p.RequestWrite(_handle, 0, _data, _offset, (int) _writeBufferSize, It.IsAny<AutoResetEvent>(), null));
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.RequestWrite(_handle, _writeBufferSize, _data, _offset + (int) _writeBufferSize, (int)_writeBufferSize, It.IsAny<AutoResetEvent>(), null));

            _sftpFileStream = new SftpFileStream(_sftpSessionMock.Object, _path, FileMode.Create, FileAccess.Write, (int) _bufferSize);
        }

        protected void Act()
        {
            _sftpFileStream.Write(_data, _offset, _count);
        }

        [TestMethod]
        public void RequestWriteOnSftpSessionShouldBeInvokedTwice()
        {
            _sftpSessionMock.Verify(p => p.RequestWrite(_handle, 0, _data, _offset, (int)_writeBufferSize, It.IsAny<AutoResetEvent>(), null), Times.Once);
            _sftpSessionMock.Verify(p => p.RequestWrite(_handle, _writeBufferSize, _data, _offset + (int)_writeBufferSize, (int)_writeBufferSize, It.IsAny<AutoResetEvent>(), null), Times.Once);
        }

        [TestMethod]
        public void PositionShouldBeNumberOfBytesWrittenToFileAndNUmberOfBytesInBuffer()
        {
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.IsOpen)
                            .Returns(true);

            Assert.AreEqual(_count, _sftpFileStream.Position);
        }

        [TestMethod]
        public void LengthShouldFlushBufferAndReturnSizeOfFile()
        {
            var lengthFileAttributes = new SftpFileAttributes(DateTime.Now, DateTime.Now, _random.Next(), _random.Next(),
                                                        _random.Next(), (uint) _random.Next(0, int.MaxValue), null);
            byte[] actualFlushedData = null;

            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.IsOpen)
                            .Returns(true);
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null))
                            .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>((handle, serverFileOffset, data, offset, length, wait, writeCompleted) => actualFlushedData = data.Take(offset, length));
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.RequestFStat(_handle, true))
                            .Returns(lengthFileAttributes);

            Assert.AreEqual(lengthFileAttributes.Size, _sftpFileStream.Length);
            Assert.IsTrue(actualFlushedData.IsEqualTo(_expectedBufferedBytes));

            _sftpSessionMock.Verify(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null), Times.Once);
        }

        [TestMethod]
        public void LengthShouldThrowIOExceptionIfRequestFStatReturnsNull()
        {
            const SftpFileAttributes lengthFileAttributes = null;
            byte[] actualFlushedData = null;

            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.IsOpen)
                            .Returns(true);
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null))
                            .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>((handle, serverFileOffset, data, offset, length, wait, writeCompleted) => actualFlushedData = data.Take(offset, length));
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.RequestFStat(_handle, true))
                            .Returns(lengthFileAttributes);

            try
            {
                var length = _sftpFileStream.Length;
                Assert.Fail();
            }
            catch (IOException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Seek operation failed.", ex.Message);
            }

            Assert.IsTrue(actualFlushedData.IsEqualTo(_expectedBufferedBytes));

            _sftpSessionMock.Verify(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null), Times.Once);
        }

        [TestMethod]
        public void LengthShouldThrowIOExceptionIfSizeIsMinusOne()
        {
            var lengthFileAttributes = new SftpFileAttributes(DateTime.Now, DateTime.Now, -1, _random.Next(), _random.Next(), (uint)_random.Next(0, int.MaxValue), null);
            byte[] actualFlushedData = null;

            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.IsOpen)
                            .Returns(true);
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null))
                            .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>((handle, serverFileOffset, data, offset, length, wait, writeCompleted) => actualFlushedData = data.Take(offset, length));
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.RequestFStat(_handle, true))
                            .Returns(lengthFileAttributes);

            try
            {
                var length = _sftpFileStream.Length;
                Assert.Fail();
            }
            catch (IOException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Seek operation failed.", ex.Message);
            }

            Assert.IsTrue(actualFlushedData.IsEqualTo(_expectedBufferedBytes));

            _sftpSessionMock.Verify(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null), Times.Once);
        }


        [TestMethod]
        public void DisposeShouldFlushBufferAndCloseRequest()
        {
            byte[] actualFlushedData = null;

            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.IsOpen)
                            .Returns(true);
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null))
                            .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>((handle, serverFileOffset, data, offset, length, wait, writeCompleted) => actualFlushedData = data.Take(offset, length));
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.RequestClose(_handle));

            _sftpFileStream.Dispose();

            Assert.IsTrue(actualFlushedData.IsEqualTo(_expectedBufferedBytes));

            _sftpSessionMock.Verify(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null), Times.Once);
            _sftpSessionMock.Verify(p => p.RequestClose(_handle), Times.Once);
        }

        [TestMethod]
        public void FlushShouldFlushBuffer()
        {
            byte[] actualFlushedData = null;

            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.IsOpen)
                            .Returns(true);
            _sftpSessionMock.InSequence(_sequence)
                            .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null))
                            .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>((handle, serverFileOffset, data, offset, length, wait, writeCompleted) => actualFlushedData = data.Take(offset, length));

            _sftpFileStream.Flush();

            Assert.IsTrue(actualFlushedData.IsEqualTo(_expectedBufferedBytes));

            _sftpSessionMock.Verify(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null), Times.Once);
        }
    }
}
