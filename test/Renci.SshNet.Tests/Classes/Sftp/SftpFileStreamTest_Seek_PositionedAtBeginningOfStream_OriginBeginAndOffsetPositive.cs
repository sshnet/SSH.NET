using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Seek_PositionedAtBeginningOfStream_OriginBeginAndOffsetPositive : SftpFileStreamTestBase
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
        private int _offset;
        private long _actual;

        protected override void SetupData()
        {
            base.SetupData();

            _random = new Random();
            _path = _random.Next().ToString();
            _fileMode = FileMode.OpenOrCreate;
            _fileAccess = FileAccess.Read;
            _bufferSize = _random.Next(5, 1000);
            _readBufferSize = (uint)_random.Next(5, 1000);
            _writeBufferSize = (uint)_random.Next(5, 1000);
            _handle = GenerateRandom(_random.Next(1, 10), _random);
            _offset = _random.Next(1, int.MaxValue);
        }

        protected override void SetupMocks()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.CreateNewOrOpen, false))
                           .Returns(_handle);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalReadLength((uint)_bufferSize))
                           .Returns(_readBufferSize);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.CalculateOptimalWriteLength((uint)_bufferSize, _handle))
                           .Returns(_writeBufferSize);
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _target = new SftpFileStream(SftpSessionMock.Object, _path, _fileMode, _fileAccess, _bufferSize);
        }

        protected override void Act()
        {
            _actual = _target.Seek(_offset, SeekOrigin.Begin);
        }

        [TestMethod]
        public void SeekShouldHaveReturnedOffset()
        {
            Assert.AreEqual(_offset, _actual);
        }

        [TestMethod]
        public void IsOpenOnSftpSessionShouldHaveBeenInvokedOnce()
        {
            SftpSessionMock.Verify(p => p.IsOpen, Times.Once);
        }

        [TestMethod]
        public void PositionShouldReturnOffset()
        {
            SftpSessionMock.InSequence(MockSequence).Setup(p => p.IsOpen).Returns(true);

            Assert.AreEqual(_offset, _target.Position);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }
    }
}
