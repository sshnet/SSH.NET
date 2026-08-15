using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// A <see cref="ConnectionHandler"/> that reproduces this library's built-in connection
    /// establishment behavior, including proxy support.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Instance"/> as the innermost handler when composing a custom
    /// <see cref="ConnectionHandler"/> chain, to retain the built-in behavior alongside your
    /// own customizations. When <see cref="ConnectionInfo.ConnectionHandler"/> is left unset,
    /// this type is not used - the built-in behavior runs as it always has, without going
    /// through this class.
    /// </remarks>
    public sealed class DefaultConnectionHandler : ConnectionHandler
    {
        private static readonly ServiceFactory DefaultServiceFactory = new();
        private static readonly SocketFactory DefaultSocketFactory = new();

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
            return DefaultServiceFactory.CreateConnector(connectionInfo, DefaultSocketFactory).Connect(connectionInfo);
        }

        /// <inheritdoc/>
        public override Task<Socket> ConnectAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            return DefaultServiceFactory.CreateConnector(connectionInfo, DefaultSocketFactory).ConnectAsync(connectionInfo, cancellationToken);
        }
    }
}
