using System.Net.Sockets;
using System.Threading;

namespace Renci.SshNet.Connection
{
    internal sealed class DirectConnector : ConnectorBase
    {
        public DirectConnector(ISocketFactory socketFactory)
            : base(socketFactory)
        {
        }

        /// <summary>
        /// Connects to a SSH endpoint using the specified <see cref="IConnectionInfo"/>.
        /// </summary>
        /// <param name="connectionInfo">The <see cref="IConnectionInfo"/> to use to establish a connection to a SSH endpoint.</param>
        /// <returns>
        /// A <see cref="Socket"/> connected to the SSH endpoint represented by the specified <see cref="IConnectionInfo"/>.
        /// </returns>
        public override Socket Connect(IConnectionInfo connectionInfo)
        {
            return SocketConnect(connectionInfo.Host, connectionInfo.Port, connectionInfo.Timeout);
        }

        /// <summary>
        /// Asynchronously connects to a SSH endpoint using the specified <see cref="IConnectionInfo"/>.
        /// </summary>
        /// <param name="connectionInfo">The <see cref="IConnectionInfo"/> to use to establish a connection to a SSH endpoint.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Socket"/> connected to the SSH endpoint represented by the specified <see cref="IConnectionInfo"/>.
        /// </returns>
        public override System.Threading.Tasks.Task<Socket> ConnectAsync(IConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            return SocketConnectAsync(connectionInfo.Host, connectionInfo.Port, cancellationToken);
        }
    }
}
