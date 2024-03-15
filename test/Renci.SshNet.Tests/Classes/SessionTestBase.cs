using Moq;
using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    internal abstract class SessionTestBase : TripleATestBase
    {
        internal Mock<IServiceFactory> ServiceFactoryMock { get; private set; }
        internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
        internal Mock<IConnector> ConnectorMock { get; private set; }

        protected virtual void CreateMocks()
        {
            ServiceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
            ConnectorMock = new Mock<IConnector>(MockBehavior.Strict);
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
