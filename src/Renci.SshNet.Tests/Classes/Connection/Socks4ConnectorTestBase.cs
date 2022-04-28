using Moq;
using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;
using System.Net;

namespace Renci.SshNet.Tests.Classes.Connection
{
    public abstract class Socks4ConnectorTestBase : TripleATestBase
    {
        internal Mock<IServiceFactory> ServiceFactoryMock { get; private set; }
        internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
        internal Socks4Connector Connector { get; private set; }
        internal SocketFactory SocketFactory { get; private set; }
        internal ServiceFactory ServiceFactory { get; private set; }

        protected virtual void CreateMocks()
        {
            ServiceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        }

        protected virtual void SetupData()
        {
            Connector = new Socks4Connector(ServiceFactoryMock.Object, SocketFactoryMock.Object);
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

        protected ConnectionInfo CreateConnectionInfo(string proxyUser, string proxyPassword, int port, int proxyPort)
        {
            return new ConnectionInfo(IPAddress.Loopback.ToString(),
                                      port,
                                      "user",
                                      ProxyTypes.Socks4,
                                      IPAddress.Loopback.ToString(),
                                      proxyPort,
                                      proxyUser,
                                      proxyPassword,
                                      new KeyboardInteractiveAuthenticationMethod("user"));
        }
    }
}
