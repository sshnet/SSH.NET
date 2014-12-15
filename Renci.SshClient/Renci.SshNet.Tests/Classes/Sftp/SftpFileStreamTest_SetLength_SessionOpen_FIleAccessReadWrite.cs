using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_SetLength_SessionOpen_FIleAccessReadWrite
    {
        private Mock<ISftpSession> _sftpSessionMock;
        private string _path;
        private SftpFileStream _sftpFileStream;
        private byte[] _handle;
        private SftpFileAttributes _fileAttributes;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private MockSequence _sequence;
        private long _length;
        private long _lengthPassedToRequestFSetStat;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        protected void Arrange()
        {
            var random = new Random();
            _path = random.Next().ToString(CultureInfo.InvariantCulture);
            _handle = new[] {(byte) random.Next(byte.MinValue, byte.MaxValue)};
            _fileAttributes = SftpFileAttributes.Empty;
            _bufferSize = (uint) random.Next(1, 1000);
            _readBufferSize = (uint) random.Next(0, 1000);
            _writeBufferSize = (uint) random.Next(0, 1000);
            _length = random.Next();

            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);

            _sequence = new MockSequence();
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Write | Flags.Truncate, true))
                .Returns(_handle);
            _sftpSessionMock.InSequence(_sequence).Setup(p => p.RequestFStat(_handle)).Returns(_fileAttributes);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                .Returns(_readBufferSize);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                .Returns(_writeBufferSize);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.IsOpen)
                .Returns(true);
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.RequestFSetStat(_handle, _fileAttributes))
                .Callback<byte[], SftpFileAttributes>((bytes, attributes) => _lengthPassedToRequestFSetStat = attributes.Size);

            _sftpFileStream = new SftpFileStream(_sftpSessionMock.Object, _path, FileMode.Create, FileAccess.ReadWrite, (int)_bufferSize);
        }

        protected void Act()
        {
            _sftpFileStream.SetLength(_length);
        }

        [TestMethod]
        public void PositionShouldReturnOriginalPosition()
        {
            _sftpSessionMock.InSequence(_sequence).Setup(p => p.IsOpen).Returns(true);

            Assert.AreEqual(0, _sftpFileStream.Position);

            _sftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public void RequestFSetStatOnSftpSessionShouldBeInvokedOnce()
        {
            _sftpSessionMock.Verify(p => p.RequestFSetStat(_handle, _fileAttributes), Times.Once);
        }

        [TestMethod]
        public void SizeOfSftpFileAttributesShouldBeModifiedToNewLengthBeforePassedToRequestFSetStat()
        {
            Assert.AreEqual(_length, _lengthPassedToRequestFSetStat);
        }

        [TestMethod]
        public void SizeOfSftpFileAttributesShouldBeEqualToNewLength()
        {
            Assert.AreEqual(_length, _fileAttributes.Size);
        }
    }
}
