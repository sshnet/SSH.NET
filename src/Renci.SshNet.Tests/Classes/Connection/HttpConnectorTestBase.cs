using Moq;
using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    public abstract class HttpConnectorTestBase : TripleATestBase
    {
        internal Mock<IServiceFactory> ServiceFactoryMock { get; private set; }
        internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
        internal HttpConnector Connector { get; private set; }
        internal SocketFactory SocketFactory { get; private set; }
        internal ServiceFactory ServiceFactory { get; private set; }

        protected virtual void CreateMocks()
        {
            ServiceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        }

        protected virtual void SetupData()
        {
            Connector = new HttpConnector(ServiceFactoryMock.Object, SocketFactoryMock.Object);
            ServiceFactory = new ServiceFactory();
            SocketFactory = new SocketFactory();
        }

        protected virtual void SetupMocks()
        {
        }
        
        protected sealed override void Arrange()
        {
            CreateMocks();
            SetupData();
            SetupMocks();
        }
    }
}
