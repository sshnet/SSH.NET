using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes
{
    public abstract class SftpClientTestBase : BaseClientTestBase
    {
        internal Mock<ISftpResponseFactory> _sftpResponseFactoryMock { get; private set; }
        internal Mock<ISftpSession> _sftpSessionMock { get; private set; }

        protected override void CreateMocks()
        {
            base.CreateMocks();

            _sftpResponseFactoryMock = new Mock<ISftpResponseFactory>(MockBehavior.Strict);
            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);
        }
    }
}
