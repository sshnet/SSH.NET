using System;
using System.Net;
using System.Text;

using Moq;

using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    public abstract class Socks5ConnectorTestBase : TripleATestBase
    {
        internal Mock<IServiceFactory> ServiceFactoryMock { get; private set; }
        internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
        internal Socks5Connector Connector { get; private set; }
        internal SocketFactory SocketFactory { get; private set; }
        internal ServiceFactory ServiceFactory { get; private set; }

        protected virtual void CreateMocks()
        {
            ServiceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        }

        protected virtual void SetupData()
        {
            Connector = new Socks5Connector(ServiceFactoryMock.Object, SocketFactoryMock.Object);
            SocketFactory = new SocketFactory();
            ServiceFactory = new ServiceFactory();
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

        protected ConnectionInfo CreateConnectionInfo(string proxyUser, string proxyPassword, int proxyPort)
        {
            return new ConnectionInfo(IPAddress.Loopback.ToString(),
                                      1029,
                                      "user",
                                      ProxyTypes.Socks5,
                                      IPAddress.Loopback.ToString(),
                                      proxyPort,
                                      proxyUser,
                                      proxyPassword,
                                      new KeyboardInteractiveAuthenticationMethod("user"));
        }

        protected static string GenerateRandomString(int minLength, int maxLength)
        {
            var random = new Random();
            var length = random.Next(minLength, maxLength);

            var sb = new StringBuilder(length);
            int offset = 'a';

            for (var i = 0; i < length; i++)
            {
                var c = (char)random.Next(offset, offset + 26);
                _ = sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
