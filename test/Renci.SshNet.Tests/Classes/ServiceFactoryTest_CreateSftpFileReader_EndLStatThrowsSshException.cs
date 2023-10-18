﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ServiceFactoryTest_CreateSftpFileReader_EndLStatThrowsSshException
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
        private ISftpFileReader _actual;

        private void SetupData()
        {
            var random = new Random();

            _bufferSize = (uint)random.Next(1, int.MaxValue);
            _openAsyncResult = new SftpOpenAsyncResult(null, null);
            _handle = CryptoAbstraction.GenerateRandom(random.Next(1, 10));
            _statAsyncResult = new SFtpStatAsyncResult(null, null);
            _fileName = random.Next().ToString();
            _chunkSize = (uint) random.Next(1, int.MaxValue);
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
                            .Throws(new SshException());
            _sftpSessionMock.InSequence(seq)
                            .Setup(p => p.CreateFileReader(_handle, _sftpSessionMock.Object, _chunkSize, 3, null))
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
