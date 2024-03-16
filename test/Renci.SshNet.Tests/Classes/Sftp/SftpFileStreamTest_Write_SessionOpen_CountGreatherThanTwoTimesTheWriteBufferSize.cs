using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Write_SessionOpen_CountGreatherThanTwoTimesTheWriteBufferSize : SftpFileStreamTestBase
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
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestOpen(_path, Flags.Write | Flags.Truncate, true))
                           .Returns(_handle);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                           .Returns(_readBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                           .Returns(_writeBufferSize);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestWrite(_handle, 0, _data, _offset, (int)_writeBufferSize, It.IsAny<AutoResetEvent>(), null));
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestWrite(_handle, _writeBufferSize, _data, _offset + (int)_writeBufferSize, (int)_writeBufferSize, It.IsAny<AutoResetEvent>(), null));
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
                               .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null));
                SftpSessionMock.InSequence(MockSequence)
                               .Setup(p => p.RequestClose(_handle));
            }
        }

        protected override void Arrange()
        {
            base.Arrange();

            _target = new SftpFileStream(SftpSessionMock.Object,
                                         _path,
                                         FileMode.Create,
                                         FileAccess.Write,
                                         (int) _bufferSize);
        }

        protected override void Act()
        {
            _target.Write(_data, _offset, _count);
        }

        [TestMethod]
        public void RequestWriteOnSftpSessionShouldBeInvokedTwice()
        {
            SftpSessionMock.Verify(p => p.RequestWrite(_handle, 0, _data, _offset, (int)_writeBufferSize, It.IsAny<AutoResetEvent>(), null), Times.Once);
            SftpSessionMock.Verify(p => p.RequestWrite(_handle, _writeBufferSize, _data, _offset + (int)_writeBufferSize, (int)_writeBufferSize, It.IsAny<AutoResetEvent>(), null), Times.Once);
        }

        [TestMethod]
        public void PositionShouldBeNumberOfBytesWrittenToFileAndNUmberOfBytesInBuffer()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            Assert.AreEqual(_count, _target.Position);
        }

        [TestMethod]
        public void LengthShouldFlushBufferAndReturnSizeOfFile()
        {
            var lengthFileAttributes = new SftpFileAttributes(DateTime.UtcNow,
                                                              DateTime.UtcNow,
                                                              _random.Next(),
                                                              _random.Next(),
                                                              _random.Next(),
                                                              (uint) _random.Next(0, int.MaxValue),
                                                              null);
            byte[] actualFlushedData = null;

            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null))
                           .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>((handle, serverFileOffset, data, offset, length, wait, writeCompleted) => actualFlushedData = data.Take(offset, length));
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestFStat(_handle, true))
                           .Returns(lengthFileAttributes);

            Assert.AreEqual(lengthFileAttributes.Size, _target.Length);
            Assert.IsTrue(actualFlushedData.IsEqualTo(_expectedBufferedBytes));

            SftpSessionMock.Verify(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null), Times.Once);
        }

        [TestMethod]
        public void LengthShouldThrowIOExceptionIfRequestFStatReturnsNull()
        {
            const SftpFileAttributes lengthFileAttributes = null;
            byte[] actualFlushedData = null;

            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null))
                           .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>((handle, serverFileOffset, data, offset, length, wait, writeCompleted) => actualFlushedData = data.Take(offset, length));
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestFStat(_handle, true))
                           .Returns(lengthFileAttributes);

            try
            {
                var length = _target.Length;
                Assert.Fail("Length should have failed, but returned: " + length + ".");
            }
            catch (IOException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Seek operation failed.", ex.Message);
            }

            Assert.IsTrue(actualFlushedData.IsEqualTo(_expectedBufferedBytes));

            SftpSessionMock.Verify(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null), Times.Once);
        }

        [TestMethod]
        public void DisposeShouldFlushBufferAndCloseRequest()
        {
            byte[] actualFlushedData = null;

            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null))
                           .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>((handle, serverFileOffset, data, offset, length, wait, writeCompleted) => actualFlushedData = data.Take(offset, length));
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestClose(_handle));

            _target.Dispose();

            Assert.IsTrue(actualFlushedData.IsEqualTo(_expectedBufferedBytes));

            SftpSessionMock.Verify(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null), Times.Once);
            SftpSessionMock.Verify(p => p.RequestClose(_handle), Times.Once);
        }

        [TestMethod]
        public void FlushShouldFlushBuffer()
        {
            byte[] actualFlushedData = null;

            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null))
                           .Callback<byte[], ulong, byte[], int, int, AutoResetEvent, Action<SftpStatusResponse>>((handle, serverFileOffset, data, offset, length, wait, writeCompleted) => actualFlushedData = data.Take(offset, length));

            _target.Flush();

            Assert.IsTrue(actualFlushedData.IsEqualTo(_expectedBufferedBytes));

            SftpSessionMock.Verify(p => p.RequestWrite(_handle, _expectedWrittenByteCount, It.IsAny<byte[]>(), 0, _expectedBufferedByteCount, It.IsAny<AutoResetEvent>(), null), Times.Once);
        }
    }
}
