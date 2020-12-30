using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using System;
using System.Net;
using System.Net.Sockets;

namespace Renci.SshNet.Connection
{
    internal abstract class ConnectorBase : IConnector
    {
        protected ConnectorBase(ISocketFactory socketFactory)
        {
            if (socketFactory == null)
                throw new ArgumentNullException("socketFactory");

            SocketFactory = socketFactory;
        }

        internal ISocketFactory SocketFactory { get; private set; }

        public abstract Socket Connect(IConnectionInfo connectionInfo);

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
    }
}
