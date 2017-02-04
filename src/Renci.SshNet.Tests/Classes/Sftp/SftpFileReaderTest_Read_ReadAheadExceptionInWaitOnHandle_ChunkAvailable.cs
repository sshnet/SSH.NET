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
    public class SftpFileReaderTest_Read_ReadAheadExceptionInWaitOnHandle_ChunkAvailable : SftpFileReaderTestBase
    {
        private const int ChunkLength = 32 * 1024;

        private MockSequence _seq;
        private byte[] _handle;
        private int _fileSize;
        private int _operationTimeout;
        private byte[] _chunk1;
        private byte[] _chunk2;
        private SftpFileReader _reader;
        private SshException _exception;
        private ManualResetEvent _exceptionSignaled;
        private SshException _actualException;

        protected override void SetupData()
        {
            var random = new Random();

            _handle = CreateByteArray(random, 5);
            _chunk1 = CreateByteArray(random, ChunkLength);
            _chunk2 = CreateByteArray(random, ChunkLength);
            _fileSize = _chunk1.Length + _chunk2.Length + 1;
            _operationTimeout = random.Next();

            _exception = new SshException();
            _exceptionSignaled = new ManualResetEvent(false);
        }

        protected override void SetupMocks()
        {
            _seq = new MockSequence();

            SftpSessionMock.InSequence(_seq).Setup(p => p.OperationTimeout).Returns(_operationTimeout);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.WaitOnHandle(It.IsAny<WaitHandle>(), _operationTimeout));
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
                           .Setup(p => p.WaitOnHandle(It.IsAny<WaitHandle>(), _operationTimeout))
                           .Callback(() => _exceptionSignaled.Set())
                           .Throws(_exception);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _reader = new SftpFileReader(_handle, SftpSessionMock.Object, ChunkLength, 2, _fileSize);
        }

        protected override void Act()
        {
            // wait for the exception to be signaled by the second call to WaitOnHandle
            _exceptionSignaled.WaitOne(5000);
            // allow a little time to allow SftpFileReader to process exception
            Thread.Sleep(100);
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
        public void ReadShouldHaveRethrownExceptionThrownByWaitOnHandle()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreSame(_exception, _actualException);
        }

        [TestMethod]
        public void ReadShouldRethrowExceptionThrownByWaitOnHandle()
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
            SftpSessionMock.InSequence(_seq).Setup(p => p.RequestClose(_handle));

            var stopwatch = Stopwatch.StartNew();
            _reader.Dispose();
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 200, "Dispose took too long to complete: " + stopwatch.ElapsedMilliseconds);

            SftpSessionMock.Verify(p => p.RequestClose(_handle), Times.Once);
        }
    }
}
