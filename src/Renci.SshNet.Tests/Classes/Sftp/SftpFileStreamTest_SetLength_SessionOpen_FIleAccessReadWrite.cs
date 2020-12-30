using System;
using System.Collections.Generic;
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
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private MockSequence _sequence;
        private long _length;
        private long _lengthPassedToRequestFSetStat;

        private DateTime _fileAttributesLastAccessTime;
        private DateTime _fileAttributesLastWriteTime;
        private long _fileAttributesSize;
        private int _fileAttributesUserId;
        private int _fileAttributesGroupId;
        private uint _fileAttributesPermissions;
        private IDictionary<string, string> _fileAttributesExtensions;
        private SftpFileAttributes _fileAttributes;

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
            _bufferSize = (uint) random.Next(1, 1000);
            _readBufferSize = (uint) random.Next(0, 1000);
            _writeBufferSize = (uint) random.Next(0, 1000);
            _length = random.Next();

            _fileAttributesLastAccessTime = DateTime.UtcNow.AddSeconds(random.Next());
            _fileAttributesLastWriteTime = DateTime.UtcNow.AddSeconds(random.Next());
            _fileAttributesSize = random.Next();
            _fileAttributesUserId = random.Next();
            _fileAttributesGroupId = random.Next();
            _fileAttributesPermissions = (uint) random.Next();
            _fileAttributesExtensions = new Dictionary<string, string>();
            _fileAttributes = new SftpFileAttributes(_fileAttributesLastAccessTime,
                                                     _fileAttributesLastWriteTime,
                                                     _fileAttributesSize,
                                                     _fileAttributesUserId,
                                                     _fileAttributesGroupId,
                                                     _fileAttributesPermissions,
                                                     _fileAttributesExtensions);

            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);

            _sequence = new MockSequence();
            _sftpSessionMock.InSequence(_sequence)
                .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Write | Flags.Truncate, true))
                .Returns(_handle);
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
                .Setup(p => p.RequestFStat(_handle, false))
                .Returns(_fileAttributes);
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
