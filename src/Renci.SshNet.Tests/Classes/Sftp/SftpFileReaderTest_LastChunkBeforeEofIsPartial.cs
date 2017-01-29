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
    public class SftpFileReaderTest_LastChunkBeforeEofIsPartial : SftpFileReaderTestBase
    {
        private const int ChunkLength = 32 * 1024;

        private byte[] _handle;
        private int _fileSize;
        private byte[] _chunk1;
        private byte[] _chunk2;
        private byte[] _chunk3;
        private SftpFileReader _reader;
        private byte[] _actualChunk1;
        private byte[] _actualChunk2;
        private byte[] _actualChunk3;

        protected override void SetupData()
        {
            var random = new Random();

            _handle = CreateByteArray(random, 5);
            _chunk1 = CreateByteArray(random, ChunkLength);
            _chunk2 = CreateByteArray(random, ChunkLength);
            _chunk3 = new byte[0];
            _fileSize = _chunk1.Length + _chunk2.Length;
        }

        protected override void SetupMocks()
        {
            var seq = new MockSequence();

            SftpSessionMock.InSequence(seq).Setup(p => p.RequestFStat(_handle)).Returns(CreateSftpFileAttributes(_chunk1.Length + _chunk2.Length));
            SftpSessionMock.InSequence(seq)
                            .Setup(p => p.BeginRead(_handle, 0, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                            .Callback<byte[], ulong, uint, AsyncCallback, object>((handle, offset, length, callback, state) =>
                            {
                                var asyncResult = new SftpReadAsyncResult(callback, state);
                                asyncResult.SetAsCompleted(_chunk1, false);
                            })
                            .Returns((SftpReadAsyncResult)null);
            SftpSessionMock.InSequence(seq)
                            .Setup(p => p.BeginRead(_handle, ChunkLength, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                            .Callback<byte[], ulong, uint, AsyncCallback, object>((handle, offset, length, callback, state) =>
                            {
                                var asyncResult = new SftpReadAsyncResult(callback, state);
                                asyncResult.SetAsCompleted(_chunk2, false);
                            })
                            .Returns((SftpReadAsyncResult)null);
            SftpSessionMock.InSequence(seq)
                            .Setup(p => p.BeginRead(_handle, 2 * ChunkLength, ChunkLength, It.IsNotNull<AsyncCallback>(), It.IsAny<BufferedRead>()))
                            .Callback<byte[], ulong, uint, AsyncCallback, object>((handle, offset, length, callback, state) =>
                            {
                                var asyncResult = new SftpReadAsyncResult(callback, state);
                                asyncResult.SetAsCompleted(_chunk3, false);
                            })
                            .Returns((SftpReadAsyncResult)null);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _reader = new SftpFileReader(_handle, SftpSessionMock.Object, 15);
        }

        protected override void Act()
        {
            _actualChunk1 = _reader.Read();
            _actualChunk2 = _reader.Read();
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
        public void ThirdReadShouldReturnChunk3()
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
        public void DisposeShouldCompleteImmediately()
        {
            var stopwatch = Stopwatch.StartNew();
            _reader.Dispose();
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 200, "Dispose took too long to complete: " + stopwatch.ElapsedMilliseconds);
        }
    }
}
