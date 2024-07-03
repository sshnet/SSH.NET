using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Connection
{
    internal abstract class ConnectorBase : IConnector, IDisposable
    {
        protected bool disposedValue;

        protected ConnectorBase(IServiceFactory serviceFactory, ISocketFactory socketFactory)
        {
            if (serviceFactory is null)
            {
                throw new ArgumentNullException(nameof(serviceFactory));
            }
            if (socketFactory is null)
            {
                throw new ArgumentNullException(nameof(socketFactory));
            }

            ServiceFactory = serviceFactory;
            SocketFactory = socketFactory;
        }

        internal IServiceFactory ServiceFactory { get; private set; }
        internal ISocketFactory SocketFactory { get; private set; }
        internal IConnector ProxyConnection { get; set; }

        public abstract Socket Connect(IConnectionInfo connectionInfo);

        public abstract Task<Socket> ConnectAsync(IConnectionInfo connectionInfo, CancellationToken cancellationToken);

        /// <summary>
        /// Establishes a socket connection to the specified host and port.
        /// </summary>
        /// <param name="host">The host name of the server to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="timeout">The maximum time to wait for the connection to be established.</param>
        /// <exception cref="SshOperationTimeoutException">The connection failed to establish within the configured <see cref="ConnectionInfo.Timeout"/>.</exception>
        /// <exception cref="SocketException">An error occurred trying to establish the connection.</exception>
        protected Socket SocketConnect(string host, int port, TimeSpan timeout)
        {
            var ipAddress = DnsAbstraction.GetHostAddresses(host)[0];
            var ep = new IPEndPoint(ipAddress, port);

            DiagnosticAbstraction.Log(string.Format("Initiating connection to '{0}:{1}'.", host, port));

            var socket = SocketFactory.Create(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                SocketAbstraction.Connect(socket, ep, timeout);

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

        /// <summary>
        /// Establishes a socket connection to the specified host and port.
        /// </summary>
        /// <param name="host">The host name of the server to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <exception cref="SshOperationTimeoutException">The connection failed to establish within the configured <see cref="ConnectionInfo.Timeout"/>.</exception>
        /// <exception cref="SocketException">An error occurred trying to establish the connection.</exception>
        protected async Task<Socket> SocketConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ipAddress = (await DnsAbstraction.GetHostAddressesAsync(host).ConfigureAwait(false))[0];
            var ep = new IPEndPoint(ipAddress, port);

            DiagnosticAbstraction.Log(string.Format("Initiating connection to '{0}:{1}'.", host, port));

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

        protected static byte SocketReadByte(Socket socket)
        {
            var buffer = new byte[1];
            _ = SocketRead(socket, buffer, 0, 1, Session.InfiniteTimeSpan);
            return buffer[0];
        }

        protected static byte SocketReadByte(Socket socket, TimeSpan readTimeout)
        {
            var buffer = new byte[1];
            _ = SocketRead(socket, buffer, 0, 1, readTimeout);
            return buffer[0];
        }

        /// <summary>
        /// Performs a blocking read on the socket until <paramref name="length"/> bytes are received.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to read from.</param>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the storage location for the received data.</param>
        /// <param name="offset">The position in <paramref name="buffer"/> parameter to store the received data.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>
        /// The number of bytes read.
        /// </returns>
        /// <exception cref="SshConnectionException">The socket is closed.</exception>
        /// <exception cref="SocketException">The read failed.</exception>
        protected static int SocketRead(Socket socket, byte[] buffer, int offset, int length)
        {
            return SocketRead(socket, buffer, offset, length, Session.InfiniteTimeSpan);
        }

        /// <summary>
        /// Performs a blocking read on the socket until <paramref name="length"/> bytes are received.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to read from.</param>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the storage location for the received data.</param>
        /// <param name="offset">The position in <paramref name="buffer"/> parameter to store the received data.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <param name="readTimeout">The maximum time to wait until <paramref name="length"/> bytes have been received.</param>
        /// <returns>
        /// The number of bytes read.
        /// </returns>
        /// <exception cref="SshConnectionException">The socket is closed.</exception>
        /// <exception cref="SshOperationTimeoutException">The read has timed-out.</exception>
        /// <exception cref="SocketException">The read failed.</exception>
        protected static int SocketRead(Socket socket, byte[] buffer, int offset, int length, TimeSpan readTimeout)
        {
            var bytesRead = SocketAbstraction.Read(socket, buffer, offset, length, readTimeout);
            if (bytesRead == 0)
            {
                throw new SshConnectionException("An established connection was aborted by the server.",
                                                 DisconnectReason.ConnectionLost);
            }

            return bytesRead;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    var proxyConnection = ProxyConnection;
                    if (proxyConnection != null)
                    {
                        proxyConnection.Dispose();
                        ProxyConnection = null;
                    }
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ConnectorBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
