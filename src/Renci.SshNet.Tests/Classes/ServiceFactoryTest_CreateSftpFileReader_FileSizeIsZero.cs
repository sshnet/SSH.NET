using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ServiceFactoryTest_CreateSftpFileReader_FileSizeIsZero
    {
        private ServiceFactory _serviceFactory;
        private Mock<ISftpSession> _sftpSessionMock;
        private Mock<ISftpFileReader> _sftpFileReaderMock;
        private uint _bufferSize;
        private string _fileName;
        private SftpOpenAsyncResult _openAsyncResult;
        private byte[] _handle;
        private SFtpStatAsyncResult _statAsyncResult;
        private uint _chunkSize;
        private long _fileSize;
        private SftpFileAttributes _fileAttributes;
        private ISftpFileReader _actual;

        private void SetupData()
        {
            var random = new Random();

            _bufferSize = (uint) random.Next(1, int.MaxValue);
            _openAsyncResult = new SftpOpenAsyncResult(null, null);
            _handle = CryptoAbstraction.GenerateRandom(random.Next(1, 10));
            _statAsyncResult = new SFtpStatAsyncResult(null, null);
            _fileName = random.Next().ToString();
            _chunkSize = (uint) random.Next(1, int.MaxValue);
            _fileSize = 0L;
            _fileAttributes = new SftpFileAttributesBuilder().WithSize(_fileSize).Build();
        }

        private void CreateMocks()
        {
            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);
            _sftpFileReaderMock = new Mock<ISftpFileReader>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            var seq = new MockSequence();

            _sftpSessionMock.InSequence(seq)
                            .Setup(p => p.BeginOpen(_fileName, Flags.Read, null, null))
                            .Returns(_openAsyncResult);
            _sftpSessionMock.InSequence(seq)
                            .Setup(p => p.EndOpen(_openAsyncResult))
                            .Returns(_handle);
            _sftpSessionMock.InSequence(seq)
                            .Setup(p => p.BeginLStat(_fileName, null, null))
                            .Returns(_statAsyncResult);
            _sftpSessionMock.InSequence(seq)
                            .Setup(p => p.CalculateOptimalReadLength(_bufferSize))
                            .Returns(_chunkSize);
            _sftpSessionMock.InSequence(seq)
                            .Setup(p => p.EndLStat(_statAsyncResult))
                            .Returns(_fileAttributes);
            _sftpSessionMock.InSequence(seq)
                            .Setup(p => p.CreateFileReader(_handle, _sftpSessionMock.Object, _chunkSize, 1, _fileSize))
                            .Returns(_sftpFileReaderMock.Object);
        }

        private void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _serviceFactory = new ServiceFactory();
        }

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        private void Act()
        {
            _actual = _serviceFactory.CreateSftpFileReader(_fileName, _sftpSessionMock.Object, _bufferSize);
        }

        [TestMethod]
        public void CreateSftpFileReaderShouldReturnCreatedInstance()
        {
            Assert.IsNotNull(_actual);
            Assert.AreSame(_sftpFileReaderMock.Object, _actual);
        }
    }
}