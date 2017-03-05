using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Ctor_FileModeAppend_FileAccessReadWrite : SftpFileStreamTestBase
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

        protected override void SetupData()
        {
            base.SetupData();

            _random = new Random();
            _path = _random.Next().ToString();
            _fileMode = FileMode.Append;
            _fileAccess = FileAccess.ReadWrite;
            _bufferSize = _random.Next(5, 1000);
            _readBufferSize = (uint) _random.Next(5, 1000);
            _writeBufferSize = (uint) _random.Next(5, 1000);
            _handle = GenerateRandom(_random.Next(1, 10), _random);
            _fileAttributes = new SftpFileAttributesBuilder().WithLastAccessTime(DateTime.Now.AddSeconds(_random.Next()))
                                                             .WithLastWriteTime(DateTime.Now.AddSeconds(_random.Next()))
                                                             .WithSize(_random.Next())
                                                             .WithUserId(_random.Next())
                                                             .WithGroupId(_random.Next())
                                                             .WithPermissions((uint)_random.Next())
                                                             .Build();
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                            .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Write | Flags.Append, false))
                            .Returns(_handle);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalReadLength((uint)_bufferSize))
                           .Returns(_readBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalWriteLength((uint)_bufferSize, _handle))
                           .Returns(_writeBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                            .Setup(p => p.RequestFStat(_handle, false))
                            .Returns(_fileAttributes);
        }

        protected override void Act()
        {
            _target = new SftpFileStream(SftpSessionMock.Object, _path, _fileMode, _fileAccess, _bufferSize);
        }

        [TestMethod]
        public void CanReadShouldReturnTrue()
        {
            Assert.IsTrue(_target.CanRead);
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
        public void ReadShouldStartReadingAtEndOfFile()
        {
            var buffer = new byte[8];
            var data = new byte[] {5, 4, 3, 2, 1};
            var expected = new byte[] {0, 5, 4, 3, 2, 1, 0, 0};

            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.RequestRead(_handle, (ulong)_fileAttributes.Size, _readBufferSize)).Returns(data);

            var actual = _target.Read(buffer, 1, data.Length);

            Assert.AreEqual(data.Length, actual);
            Assert.IsTrue(buffer.IsEqualTo(expected));

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
            SftpSessionMock.Verify(p => p.RequestRead(_handle, (ulong)_fileAttributes.Size, _readBufferSize), Times.Once);
        }

        [TestMethod]
        public void ReadByteShouldStartReadingAtEndOfFile()
        {
            var data = GenerateRandom(5, _random);

            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.RequestRead(_handle, (ulong) _fileAttributes.Size, _readBufferSize)).Returns(data);

            var actual = _target.ReadByte();

            Assert.AreEqual(data[0], actual);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
            SftpSessionMock.Verify(p => p.RequestRead(_handle, (ulong) _fileAttributes.Size, _readBufferSize), Times.Once);
        }

        [TestMethod]
        public void WriteShouldStartWritingAtEndOfFile()
        {
            var buffer = new byte[_writeBufferSize];

            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.RequestWrite(_handle, (ulong) _fileAttributes.Size, buffer, 0, buffer.Length, It.IsNotNull<AutoResetEvent>(), null));

            _target.Write(buffer, 0, buffer.Length);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
            SftpSessionMock.Verify(p => p.RequestWrite(_handle, (ulong) _fileAttributes.Size, buffer, 0, buffer.Length, It.IsNotNull<AutoResetEvent>(), null), Times.Once);
        }

        [TestMethod]
        public void RequestOpenOnSftpSessionShouldBeInvokedOnce()
        {
            SftpSessionMock.Verify(p => p.RequestOpen(_path, Flags.Read | Flags.Write | Flags.Append, false), Times.Once);
        }

        [TestMethod]
        public void RequestFStatOnSftpSessionShouldBeInvokedOnce()
        {
            SftpSessionMock.Verify(p => p.RequestFStat(_handle, false), Times.Once);
        }
    }
}
