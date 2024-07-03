using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
            {
                throw new ArgumentNullException("connectionInfo.ProxyConnection");
            }
            if (proxyConnectionInfo is not IProxyConnectionInfo)
            {
                throw new ArgumentException("Expecting ProxyConnection to be of type IProxyConnectionInfo");
            }
            return ServiceFactory.CreateConnector(proxyConnectionInfo, SocketFactory);
        }

        protected abstract void HandleProxyConnect(IConnectionInfo connectionInfo, Socket socket);

        // ToDo: Performs async/sync fallback, true async version should be implemented in derived classes
        protected virtual Task HandleProxyConnectAsync(IConnectionInfo connectionInfo, Socket socket, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (cancellationToken.Register(o => ((Socket)o).Dispose(), socket, useSynchronizationContext: false))
            {
                HandleProxyConnect(connectionInfo, socket);
            }

            return Task.CompletedTask;
        }

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
    }
}
