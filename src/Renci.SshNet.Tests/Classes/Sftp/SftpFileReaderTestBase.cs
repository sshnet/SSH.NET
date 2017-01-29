using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;
using System;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    public abstract class SftpFileReaderTestBase
    {
        internal Mock<ISftpSession> SftpSessionMock {  get; private set;}

        protected abstract void SetupData();

        protected void CreateMocks()
        {
            SftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);
        }

        protected abstract void SetupMocks();

        protected virtual void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();
        }

        [TestInitialize]
        public void SetUp()
        {
            Arrange();
            Act();
        }

        protected abstract void Act();

        protected static SftpFileAttributes CreateSftpFileAttributes(long size)
        {
            return new SftpFileAttributes(default(DateTime), default(DateTime), size, default(int), default(int), default(uint), null);
        }

        protected static byte[] CreateByteArray(Random random, int length)
        {
            var chunk = new byte[length];
            random.NextBytes(chunk);
            return chunk;
        }
    }
}
