using Moq;

using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    public abstract class DirectConnectorTestBase : TripleATestBase
    {
        internal Mock<IServiceFactory> ServiceFactoryMock { get; private set; }
        internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
        internal DirectConnector Connector { get; private set; }
        internal SocketFactory SocketFactory { get; private set; }

        protected virtual void CreateMocks()
        {
            ServiceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        }

        protected virtual void SetupData()
        {
            Connector = new DirectConnector(ServiceFactoryMock.Object, SocketFactoryMock.Object);
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

        protected ConnectionInfo CreateConnectionInfo(string hostName, int port)
        {
            return new ConnectionInfo(hostName,
                                      port,
                                      "user",
                                      new KeyboardInteractiveAuthenticationMethod("user"));
        }
    }
}
