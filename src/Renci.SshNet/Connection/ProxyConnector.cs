using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Connection
{
    internal abstract class ProxyConnector : ConnectorBase
    {
        protected ProxyConnector(ISocketFactory socketFactory)
            : base(socketFactory)
        {
        }

        protected abstract void HandleProxyConnect(IConnectionInfo connectionInfo, Socket socket);

        // ToDo: Performs async/sync fallback, true async version should be implemented in derived classes
        protected virtual Task HandleProxyConnectAsync(IConnectionInfo connectionInfo, Socket socket, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (cancellationToken.Register(o => ((Socket)o).Dispose(), socket, useSynchronizationContext: false))
            {
#pragma warning disable MA0042 // Do not use blocking calls in an async method; false positive caused by https://github.com/meziantou/Meziantou.Analyzer/issues/613
                HandleProxyConnect(connectionInfo, socket);
#pragma warning restore MA0042 // Do not use blocking calls in an async method
            }

            return Task.CompletedTask;
        }

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
