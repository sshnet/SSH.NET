using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Close_SessionNotOpen
    {
        private Mock<ISftpSession> _sftpSessionMock;
        private string _path;
        private SftpFileStream _sftpFileStream;
        private byte[] _handle;
        private SftpFileAttributes _fileAttributes;
        private uint _bufferSize;
        private uint _readBufferSize;
        private uint _writeBufferSize;

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
            _fileAttributes = SftpFileAttributes.Empty;
            _bufferSize = (uint)random.Next(1, 1000);
            _readBufferSize = (uint)random.Next(0, 1000);
            _writeBufferSize = (uint)random.Next(0, 1000);

            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _sftpSessionMock.InSequence(sequence)
                .Setup(p => p.RequestOpen(_path, Flags.Read | Flags.Truncate, true))
                .Returns(_handle);
            _sftpSessionMock.InSequence(sequence).Setup(p => p.RequestFStat(_handle, false)).Returns(_fileAttributes);
            _sftpSessionMock.InSequence(sequence)
                .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                .Returns(_readBufferSize);
            _sftpSessionMock.InSequence(sequence)
                .Setup(p => p.CalculateOptimalWriteLength(_bufferSize, _handle))
                .Returns(_writeBufferSize);
            _sftpSessionMock.InSequence(sequence)
                .Setup(p => p.IsOpen)
                .Returns(false);

            _sftpFileStream = new SftpFileStream(_sftpSessionMock.Object, _path, FileMode.Create, FileAccess.Read, (int)_bufferSize);
        }

        protected void Act()
        {
            _sftpFileStream.Close();
        }

        [TestMethod]
        public void IsOpenOnSftpSessionShouldBeInvokedOnce()
        {
            _sftpSessionMock.Verify(p => p.IsOpen, Times.Once);
        }

        [TestMethod]
        public void RequestCloseOnSftpSessionShouldNeverBeInvoked()
        {
            _sftpSessionMock.Verify(p => p.RequestClose(_handle), Times.Never);
        }
    }
}
