#if FEATURE_TAP
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_OpenAsync_FileModeAppend_FileAccessWrite : SftpFileStreamAsyncTestBase
    {
        private Random _random;
        private string _path;
        private FileMode _fileMode;
        private FileAccess _fileAccess;
        private int _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private byte[] _handle;
        private SftpFileStream _target;
        private SftpFileAttributes _fileAttributes;
        private CancellationToken _cancellationToken;

        protected override void SetupData()
        {
            base.SetupData();

            _random = new Random();
            _path = _random.Next().ToString();
            _fileMode = FileMode.Append;
            _fileAccess = FileAccess.Write;
            _bufferSize = _random.Next(5, 1000);
            _readBufferSize = (uint) _random.Next(5, 1000);
            _writeBufferSize = (uint) _random.Next(5, 1000);
            _handle = GenerateRandom(_random.Next(1, 10), _random);
            _fileAttributes = new SftpFileAttributesBuilder().WithLastAccessTime(DateTime.UtcNow.AddSeconds(_random.Next()))
                                                             .WithLastWriteTime(DateTime.UtcNow.AddSeconds(_random.Next()))
                                                             .WithSize(_random.Next())
                                                             .WithUserId(_random.Next())
                                                             .WithGroupId(_random.Next())
                                                             .WithPermissions((uint) _random.Next())
                                                             .Build();
            _cancellationToken = new CancellationToken();
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestOpenAsync(_path, Flags.Write | Flags.Append | Flags.CreateNewOrOpen, _cancellationToken))
                           .ReturnsAsync(_handle);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestFStatAsync(_handle, _cancellationToken))
                           .ReturnsAsync(_fileAttributes);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalReadLength((uint)_bufferSize))
                           .Returns(_readBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalWriteLength((uint)_bufferSize, _handle))
                           .Returns(_writeBufferSize);
        }

        protected override async Task ActAsync()
        {
            _target = await SftpFileStream.OpenAsync(SftpSessionMock.Object, _path, _fileMode, _fileAccess, _bufferSize, _cancellationToken);
        }


        [TestMethod]
        public void CanReadShouldReturnFalse()
        {
            Assert.IsFalse(_target.CanRead);
        }

        [TestMethod]
        public void CanSeekShouldReturnTrue()
        {
            Assert.IsTrue(_target.CanSeek);
        }

        [TestMethod]
        public void CanWriteShouldReturnTrue()
        {
            Assert.IsTrue(_target.CanWrite);
        }

        [TestMethod]
        public void CanTimeoutShouldReturnTrue()
        {
            Assert.IsTrue(_target.CanTimeout);
        }

        [TestMethod]
        public void PositionShouldReturnSizeOfFile()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            var actual = _target.Position;

            Assert.AreEqual(_fileAttributes.Size, actual);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
        }

        [TestMethod]
        public async Task ReadShouldThrowNotSupportedException()
        {
            var buffer = new byte[_readBufferSize];

            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            try
            {
                await _target.ReadAsync(buffer, 0, buffer.Length, _cancellationToken);
                Assert.Fail();
            }
            catch (NotSupportedException ex)
            {
                Assert.IsNull(ex.InnerException);
            }

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
        }

        [TestMethod]
        public async Task WriteShouldStartWritingAtEndOfFile()
        {
            var buffer = new byte[_writeBufferSize];

            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.RequestWriteAsync(_handle, (ulong)_fileAttributes.Size, buffer, 0, buffer.Length, _cancellationToken)).Returns(Task.CompletedTask);

            await _target.WriteAsync(buffer, 0, buffer.Length, _cancellationToken);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
            SftpSessionMock.Verify(p => p.RequestWriteAsync(_handle, (ulong)_fileAttributes.Size, buffer, 0, buffer.Length, _cancellationToken), Times.Once);
        }

        [TestMethod]
        public void RequestOpenOnSftpSessionShouldBeInvokedOnce()
        {
            SftpSessionMock.Verify(p => p.RequestOpenAsync(_path, Flags.Write | Flags.Append | Flags.CreateNewOrOpen, default), Times.Once);
        }

        [TestMethod]
        public void RequestFStatOnSftpSessionShouldBeInvokedOnce()
        {
            SftpSessionMock.Verify(p => p.RequestFStatAsync(_handle, default), Times.Once);
        }
    }
}
#endif