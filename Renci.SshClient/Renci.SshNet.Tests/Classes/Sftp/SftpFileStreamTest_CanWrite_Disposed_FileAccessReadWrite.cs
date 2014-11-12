using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_CanWrite_Disposed_FileAccessReadWrite
    {
        private Mock<ISftpSession> _sftpSessionMock;
        private string _path;
        private SftpFileStream _sftpFileStream;
        private byte[] _handle;
        private SftpFileAttributes _fileAttributes;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;
        private bool _actual;

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
            _handle = new[] { (byte)random.Next(byte.MinValue, byte.MaxValue) };
            _fileAttributes = new SftpFileAttributes();
            _bufferSize = (uint)random.Next(0, 1000);
            _readBufferSize = (uint)random.Next(0, 1000);
            _writeBufferSize = (uint)random.Next(0, 1000);

            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _sftpSessionMock.InSequence(sequence)
                .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Write | Flags.Truncate, true))
                .Returns(_handle);
            _sftpSessionMock.InSequence(sequence).Setup(p => p.RequestFStat(_handle)).Returns(_fileAttributes);
            _sftpSessionMock.InSequence(sequence)
                .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                .Returns(_readBufferSize);
            _sftpSessionMock.InSequence(sequence)
                .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                .Returns(_writeBufferSize);
            _sftpSessionMock.InSequence(sequence)
                .Setup(p => p.IsOpen)
                .Returns(true);
            _sftpSessionMock.InSequence(sequence)
                .Setup(p => p.RequestClose(_handle));

            _sftpFileStream = new SftpFileStream(_sftpSessionMock.Object, _path, FileMode.Create, FileAccess.ReadWrite, (int)_bufferSize);
            _sftpFileStream.Dispose();
        }

        protected void Act()
        {
            _actual = _sftpFileStream.CanWrite;
        }

        [TestMethod]
        public void CanWriteShouldReturnFalse()
        {
            Assert.IsFalse(_actual);
        }
    }
}
