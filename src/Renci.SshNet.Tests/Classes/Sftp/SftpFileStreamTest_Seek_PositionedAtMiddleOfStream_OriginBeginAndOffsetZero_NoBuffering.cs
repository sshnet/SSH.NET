using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    //[Ignore]
    public class SftpFileStreamTest_Seek_PositionedAtMiddleOfStream_OriginBeginAndOffsetZero_NoBuffering : SftpFileStreamTestBase
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
        private long _actual;
        private byte[] _buffer;
        private byte[] _serverData;

        protected override void SetupData()
        {
            base.SetupData();

            _random = new Random();
            _path = _random.Next().ToString();
            _fileMode = FileMode.OpenOrCreate;
            _fileAccess = FileAccess.Read;
            _bufferSize = _random.Next(5, 1000);
            _readBufferSize = 20;
            _writeBufferSize = (uint) _random.Next(5, 1000);
            _handle = GenerateRandom(_random.Next(1, 10), _random);
            _buffer = new byte[_readBufferSize];
            _serverData = GenerateRandom(_buffer.Length, _random);
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
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestRead(_handle, 0UL, _readBufferSize))
                           .Returns(_serverData);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _target = new SftpFileStream(SftpSessionMock.Object, _path, _fileMode, _fileAccess, _bufferSize);
            _target.Read(_buffer, 0, _buffer.Length);
        }

        protected override void Act()
        {
            _actual = _target.Seek(0L, SeekOrigin.Begin);
        }

        [TestMethod]
        public void SeekShouldHaveReturnedZero()
        {
            Assert.AreEqual(0L, _actual);
        }

        [TestMethod]
        public void PositionShouldReturnZero()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);

            Assert.AreEqual(0L, _target.Position);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(3));
        }

        [TestMethod]
        public void IsOpenOnSftpSessionShouldHaveBeenInvokedTwice()
        {
            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public void ReadShouldReturnReadBytesFromServer()
        {
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.IsOpen)
                           .Returns(true);
            SftpSessionMock.InSequence(MockSequence)
                           .Setup(p => p.RequestRead(_handle, 0UL, _readBufferSize))
                           .Returns(new byte[] { 0x05, 0x04 });

            var buffer = new byte[1];

            var bytesRead = _target.Read(buffer, 0, buffer.Length);

            Assert.AreEqual(buffer.Length, bytesRead);
            Assert.AreEqual(0x05, buffer[0]);

            SftpSessionMock.Verify(p => p.IsOpen, Times.Exactly(3));
            SftpSessionMock.Verify(p => p.RequestRead(_handle, 0UL, _readBufferSize), Times.Exactly(2));
        }
    }
}
