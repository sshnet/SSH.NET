using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System;
using System.Diagnostics;
using System.Threading;
using BufferedRead = Renci.SshNet.Sftp.SftpFileReader.BufferedRead;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    /// <summary>
    /// Runs a reader with max. 2 pending reads.
    /// The read-ahead of chunk1 starts followed by the read-ahead of chunk2.
    /// The read-ahead of chunk1 completes successfully and the resulting chunk is read.
    /// The read of this first chunk allows a third ahead-head to start.
    /// The second read-ahead uses signals to forcefully block a failure completion until the read
    /// ahead of the third chunk has completed and the semaphore is waiting for a slot to start
    /// the read-ahead of chunk4.
    /// The second read does not consume check3 as it is out of order, but instead waits for
    /// the outcome of the read-ahead of chunk2.
    /// 
    /// The completion with exception of chunk2 causes the second read to throw that same exception, and
    /// signals the semaphore that was waiting to start the read-ahead of chunk4. However, due to the fact
    /// that chunk2 completed with an exception, the read-ahead loop is stopped.
    /// </summary>
    [TestClass]
    public class SftpFileReaderTest_ReadAheadEndInvokeException_DiscardsFurtherReadAheads : SftpFileReaderTestBase
    {
        private const int ChunkLength = 32 * 1024;

        private MockSequence _seq;
        private byte[] _handle;
        private int _fileSize;
        private WaitHandle[] _waitHandleArray;
        private int _operationTimeout;
        private SftpCloseAsyncResult _closeAsyncResult;
        private byte[] _chunk1;
        private byte[] _chunk3;
        private ManualResetEvent _readAheadChunk2Completed;
        private ManualResetEvent _readAheadChunk3Completed;
        private ManualResetEvent _waitingForSemaphoreAfterCompletingChunk3;
        private SftpFileReader _reader;
        private SshException _exception;
        private SshException _actualException;

        protected override void SetupData()
        {
            var random = new Random();

            _handle = CreateByteArray(random, 5);
            _chunk1 = CreateByteArray(random, ChunkLength);
            _chunk3 = CreateByteArray(random, ChunkLength);
            _fileSize = 4 * ChunkLength;
            _waitHandleArray = new WaitHandle[2];
            _operationTimeout = random.Next(10000, 20000);
            _closeAsyncResult = new SftpCloseAsyncResult(null, null);

            _readAheadChunk2Completed = new ManualResetEvent(false);
            _readAheadChunk3Completed = new ManualResetEvent(false);
            _waitingForSemaphoreAfterCompletingChunk3 = new ManualResetEvent(false);

            _exception = new SshException();
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
                                ThreadAbstraction.ExecuteThread(() =>
                                {
                                    // wait until the read-ahead for chunk3 has completed; this should allow
                                    // the read-ahead of chunk4 to start
                                    _readAheadChunk3Completed.WaitOne(TimeSpan.FromSeconds(3));
                                    // wait until the semaphore wait to start with chunk4 has started
                                    _waitingForSemaphoreAfterCompletingChunk3.WaitOne(TimeSpan.FromSeconds(7));
                                    // complete async read of chunk2 with exception
                                    var asyncResult = new SftpReadAsyncResult(callback, state);
                                    asyncResult.SetAsCompleted(_exception, false);
                                    // signal that read-ahead of chunk 2 has completed
                                    _readAheadChunk2Completed.Set();
                                });
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
                                var asyncResult = new SftpReadAsyncResult(callback, state);
                                asyncResult.SetAsCompleted(_chunk3, false);
                                // signal that we've completed the read-ahead for chunk3
                                _readAheadChunk3Completed.Set();
                            })
                            .Returns((SftpReadAsyncResult)null);
            SftpSessionMock.InSequence(_seq).Setup(p => p.OperationTimeout).Returns(_operationTimeout);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.WaitAny(_waitHandleArray, _operationTimeout))
                           .Callback(() => _waitingForSemaphoreAfterCompletingChunk3.Set())
                           .Returns(() => WaitAny(_waitHandleArray, _operationTimeout));

        }

        protected override void Arrange()
        {
            base.Arrange();

            _reader = new SftpFileReader(_handle, SftpSessionMock.Object, ChunkLength, 2, _fileSize);
        }

        protected override void Act()
        {
            _reader.Read();

            try
            {
                _reader.Read();
                Assert.Fail();
            }
            catch (SshException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void ReadOfSecondChunkShouldThrowExceptionThatOccurredInReadAhead()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreSame(_exception, _actualException);
        }

        [TestMethod]
        public void ReahAheadOfChunk3ShouldHaveStarted()
        {
            SftpSessionMock.Verify(p => p.BeginRead(_handle, 2 * ChunkLength, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()), Times.Once);
        }

        [TestMethod]
        public void ReadAfterReadAheadExceptionShouldRethrowExceptionThatOccurredInReadAhead()
        {
            try
            {
                _reader.Read();
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.AreSame(_exception, ex);
            }
        }

        [TestMethod]
        public void WaitAnyOFSftpSessionShouldHaveBeenInvokedFourTimes()
        {
            SftpSessionMock.Verify(p => p.WaitAny(_waitHandleArray, _operationTimeout), Times.Exactly(4));
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
