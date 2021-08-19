using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Renci.SshNet.Connection
{
    internal sealed class DirectConnector : ConnectorBase
    {
        public DirectConnector(IServiceFactory serviceFactory, ISocketFactory socketFactory) : base(serviceFactory, socketFactory)
        {
        }

        /// <summary>
        /// Establishes a socket connection to the specified host and port.
        /// </summary>
        /// <param name="connectionInfo"><see cref="IConnectionInfo"/> object holding the configuration of the connection (Host, Port, Timeout).</param>
        /// <exception cref="SshOperationTimeoutException">The connection failed to establish within the configured <see cref="ConnectionInfo.Timeout"/>.</exception>
        /// <exception cref="SocketException">An error occurred trying to establish the connection.</exception>
        public override Socket Connect(IConnectionInfo connectionInfo)
        {            
            var ipAddress = DnsAbstraction.GetHostAddresses(connectionInfo.Host)[0];
            var ep = new IPEndPoint(ipAddress, connectionInfo.Port);

            DiagnosticAbstraction.Log(string.Format("Initiating connection to '{0}:{1}'.", connectionInfo.Host, connectionInfo.Port));

            var socket = SocketFactory.Create(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                SocketAbstraction.Connect(socket, ep, connectionInfo.Timeout);

                const int socketBufferSize = 2 * Session.MaximumSshPacketSize;
                socket.SendBufferSize = socketBufferSize;
                socket.ReceiveBufferSize = socketBufferSize;
                return socket;
            }
            catch (Exception)
            {
                socket.Dispose();
                throw;
            }
        }

#if FEATURE_TAP
        /// <summary>
        /// Establishes a socket connection to the specified host and port.
        /// </summary>
        /// <param name="connectionInfo"><see cref="IConnectionInfo"/> object holding the configuration of the connection (Host, Port).</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <exception cref="SshOperationTimeoutException">The connection failed to establish within the configured <see cref="ConnectionInfo.Timeout"/>.</exception>
        /// <exception cref="SocketException">An error occurred trying to establish the connection.</exception>
        public override async System.Threading.Tasks.Task<Socket> ConnectAsync(IConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ipAddress = (await DnsAbstraction.GetHostAddressesAsync(connectionInfo.Host).ConfigureAwait(false))[0];
            var ep = new IPEndPoint(ipAddress, connectionInfo.Port);

            DiagnosticAbstraction.Log(string.Format("Initiating connection to '{0}:{1}'.", connectionInfo.Host, connectionInfo.Port));

            var socket = SocketFactory.Create(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await SocketAbstraction.ConnectAsync(socket, ep, cancellationToken).ConfigureAwait(false);

                const int socketBufferSize = 2 * Session.MaximumSshPacketSize;
                socket.SendBufferSize = socketBufferSize;
                socket.ReceiveBufferSize = socketBufferSize;
                return socket;
            }
            catch (Exception)
            {
                socket.Dispose();
                throw;
            }
        }
#endif
    }
}
