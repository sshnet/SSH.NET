using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Seek_PositionedAtBeginningOfStream_OriginEndAndOffsetPositive : SftpFileStreamTestBase
    {
        private Random _random;
        private string _path;
        private FileMode _fileMode;
        private FileAccess _fileAccess;
        private int _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private int _length;
        private byte[] _handle;
        private SftpFileStream _target;
        private int _offset;
        private SftpFileAttributes _attributes;
        private long _actual;

        protected override void SetupData()
        {
            base.SetupData();

            _random = new Random();
            _path = _random.Next().ToString();
            _fileMode = FileMode.OpenOrCreate;
            _fileAccess = FileAccess.Write;
            _bufferSize = _random.Next(5, 1000);
            _readBufferSize = (uint)_random.Next(5, 1000);
            _writeBufferSize = (uint)_random.Next(5, 1000);
            _length = _random.Next(5, 10000);
            _handle = GenerateRandom(_random.Next(1, 10), _random);
            _offset = _random.Next(1, int.MaxValue);
            _attributes = SftpFileAttributes.Empty;
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(session => session.RequestOpen(_path, Flags.Write | Flags.CreateNewOrOpen, false))
                           .Returns(_handle);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(session => session.CalculateOptimalReadLength((uint)_bufferSize))
                           .Returns(_readBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(session => session.CalculateOptimalWriteLength((uint)_bufferSize, _handle))
                           .Returns(_writeBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(session => session.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(session => session.RequestFStat(_handle, false))
                           .Returns(_attributes);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(session => session.RequestFSetStat(_handle, _attributes));
            SftpSessionMock.InSequence(MockSequence).Setup(session => session.IsOpen).Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(session => session.RequestFStat(_handle, false))
                           .Returns(_attributes);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _target = new SftpFileStream(SftpSessionMock.Object, _path, _fileMode, _fileAccess, _bufferSize);
            _target.SetLength(_length);
        }

        protected override void Act()
        {
            _actual = _target.Seek(_offset, SeekOrigin.End);
        }

        [TestMethod]
        public void SeekShouldHaveReturnedOffset()
        {
            Assert.AreEqual(_attributes.Size + _offset, _actual);
        }

        [TestMethod]
        public void IsOpenOnSftpSessionShouldHaveBeenInvokedTwice()
        {
            SftpSessionMock.Verify(session => session.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public void PositionShouldReturnOffset()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(session => session.IsOpen).Returns(true);

            Assert.AreEqual(_attributes.Size + _offset, _target.Position);

            SftpSessionMock.Verify(session => session.IsOpen, Times.Exactly(3));
        }
    }
}
