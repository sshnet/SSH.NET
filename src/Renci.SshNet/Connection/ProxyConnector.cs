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

        protected internal IConnector GetConnector(IConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");
            if (connectionInfo.GetType() != typeof(IProxyConnectionInfo))
                throw new ArgumentException("Expecting connectionInfo to be of type IProxyConnectionInfo");
            return ServiceFactory.CreateConnector(connectionInfo.ProxyConnection, SocketFactory);
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
            ProxyConnection = GetConnector(connectionInfo);
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
            ProxyConnection = GetConnector(connectionInfo);
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
