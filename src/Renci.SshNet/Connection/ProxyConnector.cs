#if !FEATURE_SOCKET_DISPOSE
using Renci.SshNet.Common;
#endif
using System;
using System.Net.Sockets;
#if FEATURE_TAP
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Renci.SshNet.Connection
{
    internal abstract class ProxyConnector : ConnectorBase
    {
        public ProxyConnector(ISocketFactory socketFactory) :
            base(socketFactory)
        {
        }

        protected abstract void HandleProxyConnect(IConnectionInfo connectionInfo, Socket socket);

#if FEATURE_TAP
        // ToDo: Performs async/sync fallback, true async version should be implemented in derived classes
        protected virtual Task HandleProxyConnectAsync(IConnectionInfo connectionInfo, Socket socket, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (cancellationToken.Register(o => ((Socket)o).Dispose(), socket, false))
            {
                HandleProxyConnect(connectionInfo, socket);
            }
            return Task.CompletedTask;
        }
#endif

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

#if FEATURE_TAP
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
#endif
    }
}
