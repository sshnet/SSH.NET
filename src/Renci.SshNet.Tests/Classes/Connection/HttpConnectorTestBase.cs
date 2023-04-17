using Moq;
using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    public abstract class HttpConnectorTestBase : TripleATestBase
    {
        internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
        internal HttpConnector Connector { get; private set; }
        internal SocketFactory SocketFactory { get; private set; }

        protected virtual void CreateMocks()
        {
            SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        }

        protected virtual void SetupData()
        {
            Connector = new HttpConnector(SocketFactoryMock.Object);
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
