using Moq;
using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;
using System.Net;

namespace Renci.SshNet.Tests.Classes.Connection
{
    public abstract class Socks4ConnectorTestBase : TripleATestBase
    {
        internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
        internal Socks4Connector Connector { get; private set; }
        internal SocketFactory SocketFactory { get; private set; }

        protected virtual void CreateMocks()
        {
            SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        }

        protected virtual void SetupData()
        {
            Connector = new Socks4Connector(SocketFactoryMock.Object);
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

        protected ConnectionInfo CreateConnectionInfo(string proxyUser, string proxyPassword)
        {
            return new ConnectionInfo(IPAddress.Loopback.ToString(),
                                      777,
                                      "user",
                                      ProxyTypes.Socks4,
                                      IPAddress.Loopback.ToString(),
                                      8122,
                                      proxyUser,
                                      proxyPassword,
                                      new KeyboardInteractiveAuthenticationMethod("user"));
        }
    }
}
