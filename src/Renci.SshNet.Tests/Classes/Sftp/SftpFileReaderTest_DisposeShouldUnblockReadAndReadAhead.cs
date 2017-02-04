using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
        private int _operationTimeout;
        private SftpFileReader _reader;
        private ObjectDisposedException _actualException;

        protected override void SetupData()
        {
            var random = new Random();

            _handle = CreateByteArray(random, 5);
            _fileSize = 5000;
            _operationTimeout = random.Next();
        }

        protected override void SetupMocks()
        {
            _seq = new MockSequence();

            SftpSessionMock.InSequence(_seq).Setup(p => p.OperationTimeout).Returns(_operationTimeout);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.WaitOnHandle(It.IsAny<WaitHandle>(), _operationTimeout));
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.BeginRead(_handle, 0, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                           .Returns((SftpReadAsyncResult)null);
            SftpSessionMock.InSequence(_seq).Setup(p => p.OperationTimeout).Returns(_operationTimeout);
            SftpSessionMock.InSequence(_seq)
                           .Setup(p => p.WaitOnHandle(It.IsAny<WaitHandle>(), _operationTimeout));
            SftpSessionMock.InSequence(_seq).Setup(p => p.RequestClose(_handle));
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
