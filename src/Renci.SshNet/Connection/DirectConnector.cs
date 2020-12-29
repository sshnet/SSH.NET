using System.Net.Sockets;

namespace Renci.SshNet.Connection
{
    internal class DirectConnector : ConnectorBase
    {
        public DirectConnector(ISocketFactory socketFactory) : base(socketFactory)
        {
        }

        public override Socket Connect(IConnectionInfo connectionInfo)
        {
            return SocketConnect(connectionInfo.Host, connectionInfo.Port, connectionInfo.Timeout);
        }
    }
}
