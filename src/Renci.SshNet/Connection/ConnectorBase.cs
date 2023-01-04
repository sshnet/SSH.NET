using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

#if FEATURE_TAP
using System.Threading.Tasks;
#endif

namespace Renci.SshNet.Connection
{
    internal abstract class ConnectorBase : IConnector, IDisposable
    {
        protected bool disposedValue;

        protected ConnectorBase(IServiceFactory serviceFactory, ISocketFactory socketFactory)
        {
            if (serviceFactory == null)
                throw new ArgumentNullException("serviceFactory");

            if (socketFactory == null)
                throw new ArgumentNullException("socketFactory");

            ServiceFactory = serviceFactory;
            SocketFactory = socketFactory;
        }

        internal IServiceFactory ServiceFactory { get; private set; }
        internal ISocketFactory SocketFactory { get; private set; }
        internal IConnector ProxyConnection { get; set; }

        public abstract Socket Connect(IConnectionInfo connectionInfo);

#if FEATURE_TAP
        public abstract Task<Socket> ConnectAsync(IConnectionInfo connectionInfo, CancellationToken cancellationToken);
#endif

        protected static byte SocketReadByte(Socket socket)
        {
            var buffer = new byte[1];
            SocketRead(socket, buffer, 0, 1, Session.InfiniteTimeSpan);
            return buffer[0];
        }

        protected static byte SocketReadByte(Socket socket, TimeSpan readTimeout)
        {
            var buffer = new byte[1];
            SocketRead(socket, buffer, 0, 1, readTimeout);
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
