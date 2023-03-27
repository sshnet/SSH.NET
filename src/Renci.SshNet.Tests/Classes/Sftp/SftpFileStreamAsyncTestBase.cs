#if FEATURE_TAP
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    public abstract class SftpFileStreamAsyncTestBase
    {
        internal Mock<ISftpSession> SftpSessionMock;
        protected MockSequence MockSequence;

        protected virtual Task ArrangeAsync()
        {
            SetupData();
            CreateMocks();
            SetupMocks();
            return Task.CompletedTask;
        }

        protected virtual void SetupData()
        {
            MockSequence = new MockSequence();
        }

        protected abstract void SetupMocks();

        private void CreateMocks()
        {
            SftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);
        }

        [TestInitialize]
        public async Task SetUpAsync()
        {
            await ArrangeAsync();
            await ActAsync();
        }

        protected abstract Task ActAsync();

        protected byte[] GenerateRandom(int length)
        {
            return GenerateRandom(length, new Random());
        }

        protected byte[] GenerateRandom(int length, Random random)
        {
            var buffer = new byte[length];
            random.NextBytes(buffer);
            return buffer;
        }

        protected byte[] GenerateRandom(uint length)
        {
            return GenerateRandom(length, new Random());
        }

        protected byte[] GenerateRandom(uint length, Random random)
        {
            var buffer = new byte[length];
            random.NextBytes(buffer);
            return buffer;
        }
    }
}
#endif