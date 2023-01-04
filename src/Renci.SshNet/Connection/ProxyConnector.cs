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
        public ProxyConnector(IServiceFactory serviceFactory, ISocketFactory socketFactory) :
            base(serviceFactory, socketFactory)
        {
        }

        protected internal IConnector GetProxyConnector(IConnectionInfo proxyConnectionInfo)
        {
            if (proxyConnectionInfo == null)
                throw new ArgumentNullException("connectionInfo.ProxyConnection");
            if (!(proxyConnectionInfo is IProxyConnectionInfo))
                throw new ArgumentException("Expecting ProxyConnection to be of type IProxyConnectionInfo");
            return ServiceFactory.CreateConnector(proxyConnectionInfo, SocketFactory);
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
            ProxyConnection = GetProxyConnector(connectionInfo.ProxyConnection);
            var socket = ProxyConnection.Connect(connectionInfo.ProxyConnection);

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
            ProxyConnection = GetProxyConnector(connectionInfo.ProxyConnection);
            var socket = await ProxyConnection.ConnectAsync(connectionInfo.ProxyConnection, cancellationToken).ConfigureAwait(false);

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
