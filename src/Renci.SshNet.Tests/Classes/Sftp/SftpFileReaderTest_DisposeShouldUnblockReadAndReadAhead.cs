using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
#if !FEATURE_EVENTWAITHANDLE_DISPOSE
using Renci.SshNet.Common;
#endif // !FEATURE_EVENTWAITHANDLE_DISPOSE
using Renci.SshNet.Abstractions;
using Renci.SshNet.Sftp;
using System;
using System.Diagnostics;
using System.Threading;
using BufferedRead = Renci.SshNet.Sftp.SftpFileReader.BufferedRead;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileReaderTest_DisposeShouldUnblockReadAndReadAhead : SftpFileReaderTestBase
    {
        private const int ChunkLength = 32 * 1024;

        private MockSequence _seq;
        private byte[] _handle;
        private int _fileSize;
        private WaitHandle[] _waitHandleArray;
        private int _operationTimeout;
        private SftpCloseAsyncResult _closeAsyncResult;
        private SftpFileReader _reader;
        private ObjectDisposedException _actualException;
        private AsyncCallback _readAsyncCallback;
        private EventWaitHandle _disposeCompleted;

        [TestCleanup]
        public void TearDown()
        {
            if (_disposeCompleted != null)
            {
                _disposeCompleted.Dispose();
            }
        }

        protected override void SetupData()
        {
            var random = new Random();

            _handle = CreateByteArray(random, 5);
            _fileSize = 5000;
            _waitHandleArray = new WaitHandle[2];
            _operationTimeout = random.Next(10000, 20000);
            _closeAsyncResult = new SftpCloseAsyncResult(null, null);
            _disposeCompleted = new ManualResetEvent(false);
            _readAsyncCallback = null;
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
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.OperationTimeout)
                           .Returns(_operationTimeout);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.WaitAny(_waitHandleArray, _operationTimeout))
                           .Returns(() => WaitAny(_waitHandleArray, _operationTimeout));
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.BeginRead(_handle, 0, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                           .Returns<byte[], ulong, uint, AsyncCallback, object>((handle, offset, length, callback, state) =>
                                {
                                    _readAsyncCallback = callback;
                                    return null;
                                });
            SftpSessionMock.InSequence(_seq).Setup(p => p.OperationTimeout).Returns(_operationTimeout);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.WaitAny(_waitHandleArray, _operationTimeout))
                           .Returns(() => WaitAny(_waitHandleArray, _operationTimeout));
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.BeginClose(_handle, null, null))
                           .Returns(_closeAsyncResult);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.EndClose(_closeAsyncResult));
        }

        protected override void Arrange()
        {
            base.Arrange();

            _reader = new SftpFileReader(_handle, SftpSessionMock.Object, ChunkLength, 1, _fileSize);
        }

        protected override void Act()
        {
            ThreadAbstraction.ExecuteThread(() =>
            {
                Thread.Sleep(500);
                _reader.Dispose();
                _disposeCompleted.Set();
            });

            try
            {
                _reader.Read();
                Assert.Fail();
            }
            catch (ObjectDisposedException ex)
            {
                _actualException = ex;
            }

            // Dispose may unblock Read() before the dispose has fully completed, so
            // let's wait until it has completed
            _disposeCompleted.WaitOne(500);
        }

        [TestMethod]
        public void ReadShouldHaveThrownObjectDisposedException()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreEqual(typeof(SftpFileReader).FullName, _actualException.ObjectName);
        }

        [TestMethod]
        public void ReadAfterDisposeShouldThrowObjectDisposedException()
        {
            try
            {
                _reader.Read();
                Assert.Fail();
            }
            catch (ObjectDisposedException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(typeof(SftpFileReader).FullName, ex.ObjectName);
            }
        }

        [TestMethod]
        public void HandleShouldHaveBeenClosed()
        {
            SftpSessionMock.Verify(p => p.BeginClose(_handle, null, null), Times.Once);
            SftpSessionMock.Verify(p => p.EndClose(_closeAsyncResult), Times.Once);
        }

        [TestMethod]
        public void DisposeShouldCompleteImmediatelyAndNotAttemptToCloseHandleAgain()
        {
            var stopwatch = Stopwatch.StartNew();
            _reader.Dispose();
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 200, "Dispose took too long to complete: " + stopwatch.ElapsedMilliseconds);

            SftpSessionMock.Verify(p => p.BeginClose(_handle, null, null), Times.Once);
            SftpSessionMock.Verify(p => p.EndClose(_closeAsyncResult), Times.Once);
        }

        [TestMethod]
        public void InvokeOfReadAheadCallbackShouldCompleteImmediately()
        {
            Assert.IsNotNull(_readAsyncCallback);

            _readAsyncCallback(new SftpReadAsyncResult(null, null));
        }
    }
}
