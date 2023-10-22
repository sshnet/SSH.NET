using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    public abstract class SftpFileStreamAsyncTestBase
    {
        internal Mock<ISftpSession> SftpSessionMock { get; private set; }
        protected MockSequence MockSequence { get; private set; }

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
            await ArrangeAsync().ConfigureAwait(continueOnCapturedContext: false);
            await ActAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        protected static byte[] GenerateRandom(int length)
        {
            return GenerateRandom(length, new Random());
        }

        protected static byte[] GenerateRandom(int length, Random random)
        {
            if (random is null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var buffer = new byte[length];
            random.NextBytes(buffer);
            return buffer;
        }

        protected static byte[] GenerateRandom(uint length)
        {
            return GenerateRandom(length, new Random());
        }

        protected static byte[] GenerateRandom(uint length, Random random)
        {
            if (random is null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var buffer = new byte[length];
            random.NextBytes(buffer);
            return buffer;
        }

        protected abstract Task ActAsync();
    }
}
