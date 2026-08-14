using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// Represents a way to establish the underlying transport connection for an SSH session.
    /// </summary>
    /// <remarks>
    /// Implement this class to customize how the underlying <see cref="Socket"/> for a connection is
    /// established, for example to configure socket options that <see cref="ConnectionInfo"/> does not
    /// expose directly. Assign an instance to <see cref="ConnectionInfo.ConnectionHandler"/> to use it.
    /// Multiple handlers can be composed by nesting <see cref="DelegatingConnectionHandler"/>
    /// implementations, similar to <c>DelegatingHandler</c> for <c>HttpClient</c>.
    /// </remarks>
    public abstract class ConnectionHandler : IDisposable
    {
        /// <summary>
        /// Establishes a connection and returns the connected <see cref="Socket"/>.
        /// </summary>
        /// <param name="connectionInfo">The connection information.</param>
        /// <returns>
        /// A <see cref="Socket"/> connected as specified by <paramref name="connectionInfo"/>.
        /// </returns>
        public abstract Socket Connect(ConnectionInfo connectionInfo);

        /// <summary>
        /// Asynchronously establishes a connection and returns the connected <see cref="Socket"/>.
        /// </summary>
        /// <param name="connectionInfo">The connection information.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Socket"/> connected as specified by <paramref name="connectionInfo"/>.
        /// </returns>
        public abstract Task<Socket> ConnectAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
