using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
#if NET6_0_OR_GREATER == false
using System.Threading.Tasks;
#endif

using Renci.SshNet.Common;

namespace Renci.SshNet.Abstractions
{
    internal static partial class SocketAbstraction
    {
        public static void Connect(Socket socket, IPEndPoint remoteEndpoint, TimeSpan connectTimeout)
        {
            using var connectCompleted = new ManualResetEventSlim(initialState: false);
            using var args = new SocketAsyncEventArgs
            {
                RemoteEndPoint = remoteEndpoint
            };
            args.Completed += (_, _) => connectCompleted.Set();

            if (socket.ConnectAsync(args))
            {
                if (!connectCompleted.Wait(connectTimeout))
                {
                    socket.Dispose();

                    throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                                                                         "Connection failed to establish within {0:F0} milliseconds.",
                                                                         connectTimeout.TotalMilliseconds));
                }
            }

            if (args.SocketError != SocketError.Success)
            {
                var socketError = (int) args.SocketError;

                throw new SocketException(socketError);
            }
        }

        public static void ReadContinuous(Socket socket, byte[] buffer, int offset, int size, Action<byte[], int, int> processReceivedBytesAction)
        {
            // do not time-out receive
            socket.ReceiveTimeout = 0;

            while (socket.Connected)
            {
                try
                {
                    var bytesRead = socket.Receive(buffer, offset, size, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    processReceivedBytesAction(buffer, offset, bytesRead);
                }
                catch (SocketException ex)
                {
#pragma warning disable IDE0010 // Add missing cases
                    switch (ex.SocketErrorCode)
                    {
                        case SocketError.ConnectionAborted:
                        case SocketError.ConnectionReset:
                            // connection was closed
                            return;
                        case SocketError.Interrupted:
                            // connection was closed because FIN/ACK was not received in time after
                            // shutting down the (send part of the) socket
                            return;
                        default:
                            throw; // throw any other error
                    }
#pragma warning restore IDE0010 // Add missing cases
                }
            }
        }

        /// <summary>
        /// Reads a byte from the specified <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to read from.</param>
        /// <param name="timeout">Specifies the amount of time after which the call will time out.</param>
        /// <returns>
        /// The byte read, or <c>-1</c> if the socket was closed.
        /// </returns>
        /// <exception cref="SshOperationTimeoutException">The read operation timed out.</exception>
        /// <exception cref="SocketException">The read failed.</exception>
        public static int ReadByte(Socket socket, TimeSpan timeout)
        {
            var buffer = new byte[1];
            if (Read(socket, buffer, 0, 1, timeout) == 0)
            {
                return -1;
            }

            return buffer[0];
        }

        /// <summary>
        /// Receives data from a bound <see cref="Socket"/> into a receive buffer.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to read from.</param>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the storage location for the received data. </param>
        /// <param name="offset">The position in <paramref name="buffer"/> parameter to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="readTimeout">The maximum time to wait until <paramref name="size"/> bytes have been received.</param>
        /// <returns>
        /// The number of bytes received.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If no data is available for reading, the <see cref="Read(Socket, byte[], int, int, TimeSpan)"/> method will
        /// block until data is available or the time-out value is exceeded. If the time-out value is exceeded, the
        /// <see cref="Read(Socket, byte[], int, int, TimeSpan)"/> call will throw a <see cref="SshOperationTimeoutException"/>.
        /// </para>
        /// </remarks>
        public static int Read(Socket socket, byte[] buffer, int offset, int size, TimeSpan readTimeout)
        {
            var totalBytesRead = 0;
            var totalBytesToRead = size;

            socket.ReceiveTimeout = readTimeout.AsTimeout(nameof(readTimeout));

            do
            {
                try
                {
                    var bytesRead = socket.Receive(buffer, offset + totalBytesRead, totalBytesToRead - totalBytesRead, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        return 0;
                    }

                    totalBytesRead += bytesRead;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                                                            "Socket read operation has timed out after {0:F0} milliseconds.",
                                                            readTimeout.TotalMilliseconds),
                                                            ex);
                }
            }
            while (totalBytesRead < totalBytesToRead);

            return totalBytesRead;
        }

#if NET6_0_OR_GREATER == false
        public static ValueTask<int> ReadAsync(Socket socket, byte[] buffer, CancellationToken cancellationToken)
        {
            return socket.ReceiveAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), SocketFlags.None, cancellationToken);
        }
#endif
    }
}
