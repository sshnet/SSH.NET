using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_OpenAsync_FileModeCreate_FileAccessWrite_FileDoesNotExist : SftpFileStreamAsyncTestBase
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
        private CancellationToken _cancellationToken;

        protected override void SetupData()
        {
            base.SetupData();

            _random = new Random();
            _path = _random.Next().ToString(CultureInfo.InvariantCulture);
            _fileMode = FileMode.Create;
            _fileAccess = FileAccess.Write;
            _bufferSize = _random.Next(5, 1000);
            _readBufferSize = (uint)_random.Next(5, 1000);
            _writeBufferSize = (uint)_random.Next(5, 1000);
            _handle = GenerateRandom(_random.Next(1, 10), _random);
            _cancellationToken = default;
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestOpenAsync(_path, Flags.Write | Flags.CreateNewOrOpen | Flags.Truncate, _cancellationToken))
                           .ReturnsAsync(_handle);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalReadLength((uint) _bufferSize))
                           .Returns(_readBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalWriteLength((uint) _bufferSize, _handle))
                           .Returns(_writeBufferSize);
        }

        protected override async Task ActAsync()
        {
            _target = await SftpFileStream.OpenAsync(SftpSessionMock.Object, _path, _fileMode, _fileAccess, _bufferSize, _cancellationToken)
                                          .ConfigureAwait(continueOnCapturedContext: false);
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
        public void PositionShouldReturnZero()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(value: true);

            var actual = _target.Position;

            Assert.AreEqual(0L, actual);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
        }

        [TestMethod]
        public async Task ReadShouldThrowNotSupportedException()
        {
            var buffer = new byte[_readBufferSize];

            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(value: true);

            try
            {
#pragma warning disable MA0040 // Forward the CancellationToken parameter to methods that take one
#pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
                _ = await _target.ReadAsync(buffer, 0, buffer.Length)
                                 .ConfigureAwait(continueOnCapturedContext: false);
#pragma warning restore CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
#pragma warning restore MA0040 // Forward the CancellationToken parameter to methods that take one
                Assert.Fail();
            }
            catch (NotSupportedException ex)
            {
                Assert.IsNull(ex.InnerException);
            }

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
        }

        [TestMethod]
        public async Task WriteShouldStartWritingAtBeginningOfFile()
        {
            var buffer = new byte[_writeBufferSize];

            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(value: true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestWriteAsync(_handle, 0UL, buffer, 0, buffer.Length, _cancellationToken))
                           .Returns(Task.CompletedTask);

#pragma warning disable MA0040 // Forward the CancellationToken parameter to methods that take one
#pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
            await _target.WriteAsync(buffer, 0, buffer.Length)
                         .ConfigureAwait(continueOnCapturedContext: false);
#pragma warning restore CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
#pragma warning restore MA0040 // Forward the CancellationToken parameter to methods that take one

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
            SftpSessionMock.Verify(p => p.RequestWriteAsync(_handle, 0UL, buffer, 0, buffer.Length, _cancellationToken), Times.Once);
        }

        [TestMethod]
        public void RequestOpenOnSftpSessionShouldBeInvokedOnceWithTruncateAndOnceWithCreateNew()
        {
            SftpSessionMock.Verify(p => p.RequestOpenAsync(_path, Flags.Write | Flags.CreateNewOrOpen | Flags.Truncate, _cancellationToken), Times.Once);
        }
    }
}
