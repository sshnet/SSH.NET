using Moq;
using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    public abstract class BaseClientTestBase : TripleATestBase
    {
        internal Mock<IServiceFactory> _serviceFactoryMock { get; private set; }
        internal Mock<ISocketFactory> _socketFactoryMock { get; private set; }
        internal Mock<ISession> _sessionMock { get; private set; }

        protected virtual void CreateMocks()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _socketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        }

        protected virtual void SetupData()
        {
        }

        protected virtual void SetupMocks()
        {
        }

        protected override void Arrange()
        {
            CreateMocks();
            SetupData();
            SetupMocks();
        }
    }
}
