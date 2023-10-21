using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// Represents a connector that uses a proxy server to establish a connection to a given SSH
    /// endpoint.
    /// </summary>
    internal abstract class ProxyConnector : ConnectorBase
    {
        protected ProxyConnector(ISocketFactory socketFactory)
            : base(socketFactory)
        {
        }

        protected abstract void HandleProxyConnect(IConnectionInfo connectionInfo, Socket socket);

        // ToDo: Performs async/sync fallback, true async version should be implemented in derived classes
        protected virtual
#if NET || NETSTANDARD2_1_OR_GREATER
        async
#endif // NET || NETSTANDARD2_1_OR_GREATER
        Task HandleProxyConnectAsync(IConnectionInfo connectionInfo, Socket socket, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

#if NET || NETSTANDARD2_1_OR_GREATER
            await using (cancellationToken.Register(o => ((Socket)o).Dispose(), socket, useSynchronizationContext: false).ConfigureAwait(continueOnCapturedContext: false))
#else
            using (cancellationToken.Register(o => ((Socket) o).Dispose(), socket, useSynchronizationContext: false))
#endif // NET || NETSTANDARD2_1_OR_GREATER
            {
#pragma warning disable MA0042 // Do not use blocking calls in an async method; false positive caused by https://github.com/meziantou/Meziantou.Analyzer/issues/613
                HandleProxyConnect(connectionInfo, socket);
#pragma warning restore MA0042 // Do not use blocking calls in an async method
            }

#if !NET && !NETSTANDARD2_1_OR_GREATER
            return Task.CompletedTask;
#endif // !NET && !NETSTANDARD2_1_OR_GREATER
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
            var socket = SocketConnect(connectionInfo.ProxyHost, connectionInfo.ProxyPort, connectionInfo.Timeout);

            try
            {
                HandleProxyConnect(connectionInfo, socket);
                return socket;
            }
            catch (Exception)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Asynchronously connects to a SSH endpoint using the specified <see cref="IConnectionInfo"/>.
        /// </summary>
        /// <param name="connectionInfo">The <see cref="IConnectionInfo"/> to use to establish a connection to a SSH endpoint.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Socket"/> connected to the SSH endpoint represented by the specified <see cref="IConnectionInfo"/>.
        /// </returns>
        public override async Task<Socket> ConnectAsync(IConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            var socket = await SocketConnectAsync(connectionInfo.ProxyHost, connectionInfo.ProxyPort, cancellationToken).ConfigureAwait(false);

            try
            {
                await HandleProxyConnectAsync(connectionInfo, socket, cancellationToken).ConfigureAwait(false);
                return socket;
            }
            catch (Exception)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();

                throw;
            }
        }
    }
}
