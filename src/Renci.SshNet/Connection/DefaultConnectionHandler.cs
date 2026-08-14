using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// The <see cref="ConnectionHandler"/> used when no custom <see cref="ConnectionInfo.ConnectionHandler"/>
    /// is configured.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Instance"/> as the innermost handler when composing a custom
    /// <see cref="ConnectionHandler"/> chain, to retain this library's built-in connection
    /// establishment behavior (including proxy support) alongside your own customizations.
    /// </remarks>
    public sealed class DefaultConnectionHandler : ConnectionHandler
    {
        private static readonly ServiceFactory ServiceFactory = new();
        private static readonly SocketFactory SocketFactory = new();

        /// <summary>
        /// Gets the singleton instance of <see cref="DefaultConnectionHandler"/>.
        /// </summary>
        public static DefaultConnectionHandler Instance { get; } = new DefaultConnectionHandler();

        private DefaultConnectionHandler()
        {
        }

        /// <inheritdoc/>
        public override Socket Connect(ConnectionInfo connectionInfo)
        {
            return ServiceFactory.CreateConnector(connectionInfo, SocketFactory).Connect(connectionInfo);
        }

        /// <inheritdoc/>
        public override Task<Socket> ConnectAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            return ServiceFactory.CreateConnector(connectionInfo, SocketFactory).ConnectAsync(connectionInfo, cancellationToken);
        }
    }
}
