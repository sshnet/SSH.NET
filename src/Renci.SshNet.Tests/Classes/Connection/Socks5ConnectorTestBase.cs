using Moq;
using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;
using System;
using System.Net;
using System.Text;

namespace Renci.SshNet.Tests.Classes.Connection
{
    public abstract class Socks5ConnectorTestBase : TripleATestBase
    {
        internal Mock<ISocketFactory> SocketFactoryMock { get; private set; }
        internal Socks5Connector Connector { get; private set; }
        internal SocketFactory SocketFactory { get; private set; }

        protected virtual void CreateMocks()
        {
            SocketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        }

        protected virtual void SetupData()
        {
            Connector = new Socks5Connector(SocketFactoryMock.Object);
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
                                      ProxyTypes.Socks5,
                                      IPAddress.Loopback.ToString(),
                                      8122,
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
                var @char = (char) random.Next(offset, offset + 26);
                sb.Append(@char);
            }

            return sb.ToString();
        }
    }
}
