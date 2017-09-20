using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System;
using System.Diagnostics;
using System.Threading;
using BufferedRead = Renci.SshNet.Sftp.SftpFileReader.BufferedRead;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileReaderTest_PreviousChunkIsIncompleteAndEofIsReached : SftpFileReaderTestBase
    {
        private const int ChunkLength = 32 * 1024;

        private MockSequence _seq;
        private byte[] _handle;
        private int _fileSize;
        private WaitHandle[] _waitHandleArray;
        private int _operationTimeout;
        private SftpCloseAsyncResult _closeAsyncResult;
        private byte[] _chunk1;
        private byte[] _chunk2;
        private byte[] _chunk2CatchUp;
        private byte[] _chunk3;
        private SftpFileReader _reader;
        private byte[] _actualChunk1;
        private byte[] _actualChunk2;
        private byte[] _actualChunk2CatchUp;
        private byte[] _actualChunk3;
        private ManualResetEvent _chunk1BeginRead;
        private ManualResetEvent _chunk2BeginRead;
        private ManualResetEvent _chunk3BeginRead;

        protected override void SetupData()
        {
            var random = new Random();

            _handle = CreateByteArray(random, 3);
            _chunk1 = CreateByteArray(random, ChunkLength);
            _chunk2 = CreateByteArray(random, ChunkLength - 10);
            _chunk2CatchUp = CreateByteArray(random, 10);
            _chunk3 = new byte[0];
            _chunk1BeginRead = new ManualResetEvent(false);
            _chunk2BeginRead = new ManualResetEvent(false);
            _chunk3BeginRead = new ManualResetEvent(false);
            _fileSize = _chunk1.Length + _chunk2.Length + _chunk2CatchUp.Length + _chunk3.Length;
            _waitHandleArray = new WaitHandle[2];
            _operationTimeout = random.Next(10000, 20000);
            _closeAsyncResult = new SftpCloseAsyncResult(null, null);
        }

        protected override void SetupMocks()
        {
            _seq = new MockSequence();

            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.CreateWaitHandleArray(It.IsNotNull<WaitHandle>(), It.IsNotNull<WaitHandle>()))
                           .Returns<WaitHandle, WaitHandle>((disposingWaitHandle, semaphoreAvailableWaitHandle) =>
                           {
                               _waitHandleArray[0] = disposingWaitHandle;
                               _waitHandleArray[1] = semaphoreAvailableWaitHandle;
                               return _waitHandleArray;
                           });
            SftpSessionMock.InSequence(_seq).Setup(p => p.OperationTimeout).Returns(_operationTimeout);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.WaitAny(_waitHandleArray, _operationTimeout))
                           .Returns(() => WaitAny(_waitHandleArray, _operationTimeout));
            SftpSessionMock.InSequence(_seq)
                            .Setup(p => p.BeginRead(_handle, 0, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                            .Callback<byte[], ulong, uint, AsyncCallback, object>((handle, offset, length, callback, state) =>
                            {
                                _chunk1BeginRead.Set();
                                var asyncResult = new SftpReadAsyncResult(callback, state);
                                asyncResult.SetAsCompleted(_chunk1, false);
                            })
                            .Returns((SftpReadAsyncResult)null);
            SftpSessionMock.InSequence(_seq).Setup(p => p.OperationTimeout).Returns(_operationTimeout);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.WaitAny(_waitHandleArray, _operationTimeout))
                           .Returns(() => WaitAny(_waitHandleArray, _operationTimeout));
            SftpSessionMock.InSequence(_seq)
                            .Setup(p => p.BeginRead(_handle, ChunkLength, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                            .Callback<byte[], ulong, uint, AsyncCallback, object>((handle, offset, length, callback, state) =>
                            {
                                _chunk2BeginRead.Set();
                                var asyncResult = new SftpReadAsyncResult(callback, state);
                                asyncResult.SetAsCompleted(_chunk2, false);
                            })
                            .Returns((SftpReadAsyncResult)null);
            SftpSessionMock.InSequence(_seq).Setup(p => p.OperationTimeout).Returns(_operationTimeout);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.WaitAny(_waitHandleArray, _operationTimeout))
                           .Returns(() => WaitAny(_waitHandleArray, _operationTimeout));
            SftpSessionMock.InSequence(_seq)
                            .Setup(p => p.BeginRead(_handle, 2 * ChunkLength, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                            .Callback<byte[], ulong, uint, AsyncCallback, object>((handle, offset, length, callback, state) =>
                            {
                                _chunk3BeginRead.Set();
                                var asyncResult = new SftpReadAsyncResult(callback, state);
                                asyncResult.SetAsCompleted(_chunk3, false);
                            })
                            .Returns((SftpReadAsyncResult)null);
            SftpSessionMock.InSequence(_seq)
                            .Setup(p => p.RequestRead(_handle, 2 * ChunkLength - 10, 10))
                            .Returns(_chunk2CatchUp);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _reader = new SftpFileReader(_handle, SftpSessionMock.Object, ChunkLength, 5, _fileSize);
        }

        protected override void Act()
        {
            // consume chunk 1
            _actualChunk1 = _reader.Read();
            // consume chunk 2
            _actualChunk2 = _reader.Read();
            // wait until chunk3 has been read-ahead
            Assert.IsTrue(_chunk3BeginRead.WaitOne(200));
            // consume remaining parts of chunk 2
            _actualChunk2CatchUp = _reader.Read();
            // consume chunk 3
            _actualChunk3 = _reader.Read();
        }

        [TestMethod]
        public void FirstReadShouldReturnChunk1()
        {
            Assert.IsNotNull(_actualChunk1);
            Assert.AreSame(_chunk1, _actualChunk1);
        }

        [TestMethod]
        public void SecondReadShouldReturnChunk2()
        {
            Assert.IsNotNull(_actualChunk2);
            Assert.AreSame(_chunk2, _actualChunk2);
        }

        [TestMethod]
        public void ThirdReadShouldReturnChunk2CatchUp()
        {
            Assert.IsNotNull(_actualChunk2CatchUp);
            Assert.AreSame(_chunk2CatchUp, _actualChunk2CatchUp);
        }

        [TestMethod]
        public void FourthReadShouldReturnChunk3()
        {
            Assert.IsNotNull(_actualChunk3);
            Assert.AreSame(_chunk3, _actualChunk3);
        }

        [TestMethod]
        public void ReadAfterEndOfFileShouldThrowSshException()
        {
            try
            {
                _reader.Read();
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Attempting to read beyond the end of the file.", ex.Message);
            }
        }

        [TestMethod]
        public void DisposeShouldCloseHandleAndCompleteImmediately()
        {
            SftpSessionMock.InSequence(_seq).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(_seq).Setup(p => p.BeginClose(_handle, null, null)).Returns(_closeAsyncResult);
            SftpSessionMock.InSequence(_seq).Setup(p => p.EndClose(_closeAsyncResult));

            var stopwatch = Stopwatch.StartNew();
            _reader.Dispose();
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 200, "Dispose took too long to complete: " + stopwatch.ElapsedMilliseconds);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Once);
            SftpSessionMock.Verify(p => p.BeginClose(_handle, null, null), Times.Once);
            SftpSessionMock.Verify(p => p.EndClose(_closeAsyncResult), Times.Once);
        }
    }
}
