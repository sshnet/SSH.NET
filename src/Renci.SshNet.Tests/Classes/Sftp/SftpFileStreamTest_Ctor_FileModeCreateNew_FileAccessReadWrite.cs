using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Ctor_FileModeCreateNew_FileAccessReadWrite : SftpFileStreamTestBase
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

        protected override void SetupData()
        {
            base.SetupData();

            _random = new Random();
            _path = _random.Next().ToString();
            _fileMode = FileMode.CreateNew;
            _fileAccess = FileAccess.ReadWrite;
            _bufferSize = _random.Next(5, 1000);
            _readBufferSize = (uint)_random.Next(5, 1000);
            _writeBufferSize = (uint)_random.Next(5, 1000);
            _handle = GenerateRandom(_random.Next(1, 10), _random);
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Write | Flags.CreateNew, false))
                           .Returns(_handle);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalReadLength((uint) _bufferSize))
                           .Returns(_readBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalWriteLength((uint) _bufferSize, _handle))
                           .Returns(_writeBufferSize);
        }

        protected override void Act()
        {
            _target = new SftpFileStream(SftpSessionMock.Object,
                                         _path,
                                         _fileMode,
                                         _fileAccess,
                                         _bufferSize);
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
        public void PositionShouldReturnZero()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            var actual = _target.Position;

            Assert.AreEqual(0L, actual);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
        }

        [TestMethod]
        public void ReadShouldStartReadingAtBeginningOfFile()
        {
            var buffer = new byte[8];
            var data = new byte[] { 5, 4, 3, 2, 1 };
            var expected = new byte[] { 0, 5, 4, 3, 2, 1, 0, 0 };

            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.RequestRead(_handle, 0UL, _readBufferSize)).Returns(data);

            var actual = _target.Read(buffer, 1, data.Length);

            Assert.AreEqual(data.Length, actual);
            Assert.IsTrue(buffer.IsEqualTo(expected));

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
            SftpSessionMock.Verify(p => p.RequestRead(_handle, 0UL, _readBufferSize), Times.Once);
        }

        [TestMethod]
        public void ReadByteShouldStartReadingAtBeginningOfFile()
        {
            var data = GenerateRandom(5, _random);

            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.RequestRead(_handle, 0UL, _readBufferSize))
                .Returns(data);

            var actual = _target.ReadByte();

            Assert.AreEqual(data[0], actual);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
            SftpSessionMock.Verify(p => p.RequestRead(_handle, 0UL, _readBufferSize), Times.Once);
        }

        [TestMethod]
        public void WriteShouldStartWritingAtBeginningOfFile()
        {
            var buffer = new byte[_writeBufferSize];

            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.RequestWrite(_handle, 0UL, buffer, 0, buffer.Length, It.IsNotNull<AutoResetEvent>(), null));

            _target.Write(buffer, 0, buffer.Length);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(1));
            SftpSessionMock.Verify(p => p.RequestWrite(_handle, 0UL, buffer, 0, buffer.Length, It.IsNotNull<AutoResetEvent>(), null), Times.Once);
        }

        [TestMethod]
        public void RequestOpenOnSftpSessionShouldBeInvokedOnce()
        {
            SftpSessionMock.Verify(p => p.RequestOpen(_path, Flags.Read | Flags.Write | Flags.CreateNew, false), Times.Once);
        }
    }
}
