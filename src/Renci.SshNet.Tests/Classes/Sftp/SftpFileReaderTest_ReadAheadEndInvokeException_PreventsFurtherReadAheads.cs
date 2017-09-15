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
    [TestClass]
    public class SftpFileReaderTest_ReadAheadEndInvokeException_PreventsFurtherReadAheads : SftpFileReaderTestBase
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
        private SftpFileReader _reader;
        private ManualResetEvent _readAheadChunk2;
        private ManualResetEvent _readChunk2;
        private SshException _exception;
        private SshException _actualException;

        protected override void SetupData()
        {
            var random = new Random();

            _handle = CreateByteArray(random, 5);
            _chunk1 = CreateByteArray(random, ChunkLength);
            _chunk3 = CreateByteArray(random, ChunkLength);
            _fileSize = 3 * _chunk1.Length;
            _waitHandleArray = new WaitHandle[2];
            _operationTimeout = random.Next(10000, 20000);
            _closeAsyncResult = new SftpCloseAsyncResult(null, null);

            _readAheadChunk2 = new ManualResetEvent(false);
            _readChunk2 = new ManualResetEvent(false);

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
                                    // signal that we're in the read-ahead for chunk2
                                    _readAheadChunk2.Set();
                                    // wait for client to start reading this chunk
                                    _readChunk2.WaitOne(TimeSpan.FromSeconds(5));
                                    // sleep a short time to make sure the client is in the blocking wait
                                    Thread.Sleep(500);
                                    // complete async read of chunk2 with exception
                                    var asyncResult = new SftpReadAsyncResult(callback, state);
                                    asyncResult.SetAsCompleted(_exception, false);
                                });
                            })
                           .Returns((SftpReadAsyncResult)null);
            SftpSessionMock.InSequence(_seq).Setup(p => p.OperationTimeout).Returns(_operationTimeout);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.WaitAny(_waitHandleArray, _operationTimeout))
                           .Returns(() => WaitAny(_waitHandleArray, _operationTimeout));
        }

        protected override void Arrange()
        {
            base.Arrange();

            // use a max. read-ahead of 1 to allow us to verify that the next read-ahead is not done
            // when a read-ahead has failed
            _reader = new SftpFileReader(_handle, SftpSessionMock.Object, ChunkLength, 1, _fileSize);
        }

        protected override void Act()
        {
            _reader.Read();

            // wait until SftpFileReader has starting reading ahead chunk 2
            Assert.IsTrue(_readAheadChunk2.WaitOne(TimeSpan.FromSeconds(5)));
            // signal that we are about to read chunk 2
            _readChunk2.Set();

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

        [TestMethod]
        public void ExceptionInReadAheadShouldPreventFurtherReadAheads()
        {
            SftpSessionMock.Verify(p => p.BeginRead(_handle, 2 * ChunkLength, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()), Times.Never);
        }
    }
}
