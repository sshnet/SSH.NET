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
    public class SftpFileReaderTest_Read_ExceptionInReadAhead : SftpFileReaderTestBase
    {
        private const int ChunkLength = 32 * 1024;

        private MockSequence _seq;
        private byte[] _handle;
        private int _fileSize;
        private byte[] _chunk1;
        private byte[] _chunk2;
        private SftpFileReader _reader;
        private ManualResetEvent _readAhead;
        private ManualResetEvent _readChunk3;
        private SshException _exception;
        private SshException _actualException;

        protected override void SetupData()
        {
            var random = new Random();

            _handle = CreateByteArray(random, 5);
            _chunk1 = CreateByteArray(random, ChunkLength);
            _chunk2 = CreateByteArray(random, ChunkLength);
            _fileSize = _chunk1.Length + _chunk2.Length + 1;

            _readAhead = new ManualResetEvent(false);
            _readChunk3 = new ManualResetEvent(false);

            _exception = new SshException();
        }

        protected override void SetupMocks()
        {
            _seq = new MockSequence();

            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.BeginRead(_handle, 0, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                           .Callback<byte[], ulong, uint, AsyncCallback, object>((handle, offset, length, callback, state) =>
                                {
                                    var asyncResult = new SftpReadAsyncResult(callback, state);
                                    asyncResult.SetAsCompleted(_chunk1, false);
                                })
                           .Returns((SftpReadAsyncResult)null);
            SftpSessionMock.InSequence(_seq)
                            .Setup(p => p.BeginRead(_handle, ChunkLength, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                            .Callback<byte[], ulong, uint, AsyncCallback, object>((handle, offset, length, callback, state) =>
                            {
                                var asyncResult = new SftpReadAsyncResult(callback, state);
                                asyncResult.SetAsCompleted(_chunk2, false);
                            })
                            .Returns((SftpReadAsyncResult)null);
            SftpSessionMock.InSequence(_seq)
                            .Setup(p => p.BeginRead(_handle, 2 * ChunkLength, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                            .Callback<byte[], ulong, uint, AsyncCallback, object>((handle, offset, length, callback, state) =>
                            {
                                _readAhead.Set();
                                _readChunk3.WaitOne(TimeSpan.FromSeconds(5));
                                // sleep a short time to make sure the client is in the blocking wait
                                Thread.Sleep(500);
                            })
                            .Throws(_exception);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _reader = new SftpFileReader(_handle, SftpSessionMock.Object, ChunkLength, 3, _fileSize);
        }

        protected override void Act()
        {
            _reader.Read();
            _reader.Read();

            // wait until we've the SftpFileReader has starting reading ahead chunk 3
            Assert.IsTrue(_readAhead.WaitOne(TimeSpan.FromSeconds(5)));
            // signal that we are about to read chunk 3
            _readChunk3.Set();

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
        public void ReadOfThirdChunkShouldThrowExceptionThatOccurredInReadAhead()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreSame(_exception, _actualException);
        }

        [TestMethod]
        public void ReadAfterReadAheadExceptionShouldThrowObjectDisposedException()
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
