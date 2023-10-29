using System.Net.Sockets;
using System.Threading;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// Represents a means to connect to a SSH endpoint.
    /// </summary>
    internal interface IConnector
    {
        /// <summary>
        /// Connects to a SSH endpoint using the specified <see cref="IConnectionInfo"/>.
        /// </summary>
        /// <param name="connectionInfo">The <see cref="IConnectionInfo"/> to use to establish a connection to a SSH endpoint.</param>
        /// <returns>
        /// A <see cref="Socket"/> connected to the SSH endpoint represented by the specified <see cref="IConnectionInfo"/>.
        /// </returns>
        Socket Connect(IConnectionInfo connectionInfo);

        /// <summary>
        /// Asynchronously connects to a SSH endpoint using the specified <see cref="IConnectionInfo"/>.
        /// </summary>
        /// <param name="connectionInfo">The <see cref="IConnectionInfo"/> to use to establish a connection to a SSH endpoint.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Socket"/> connected to the SSH endpoint represented by the specified <see cref="IConnectionInfo"/>.
        /// </returns>
        System.Threading.Tasks.Task<Socket> ConnectAsync(IConnectionInfo connectionInfo, CancellationToken cancellationToken);
    }
}
