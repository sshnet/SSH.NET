using System.Net.Sockets;
using System.Threading;

namespace Renci.SshNet.Connection
{
    internal sealed class DirectConnector : ConnectorBase
    {
        public DirectConnector(ISocketFactory socketFactory) : base(socketFactory)
        {
        }

        public override Socket Connect(IConnectionInfo connectionInfo)
        {
            return SocketConnect(connectionInfo.Host, connectionInfo.Port, connectionInfo.Timeout);
        }

#if FEATURE_TAP
        public override System.Threading.Tasks.Task<Socket> ConnectAsync(IConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            return SocketConnectAsync(connectionInfo.Host, connectionInfo.Port, cancellationToken);
        }
#endif
    }
}
