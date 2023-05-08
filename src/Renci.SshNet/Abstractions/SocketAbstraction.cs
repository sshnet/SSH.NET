using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Abstractions
{
    internal static class SocketAbstraction
    {
        public static bool CanRead(Socket socket)
        {
            if (socket.Connected)
            {
                return socket.Poll(-1, SelectMode.SelectRead) && socket.Available > 0;
            }

            return false;

        }

        /// <summary>
        /// Returns a value indicating whether the specified <see cref="Socket"/> can be used
        /// to send data.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to check.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="socket"/> can be written to; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanWrite(Socket socket)
        {
            if (socket != null && socket.Connected)
            {
                return socket.Poll(-1, SelectMode.SelectWrite);
            }

            return false;
        }

        public static Socket Connect(IPEndPoint remoteEndpoint, TimeSpan connectTimeout)
        {
            var socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            ConnectCore(socket, remoteEndpoint, connectTimeout, true);
            return socket;
        }

        public static void Connect(Socket socket, IPEndPoint remoteEndpoint, TimeSpan connectTimeout)
        {
            ConnectCore(socket, remoteEndpoint, connectTimeout, false);
        }

        public static async Task ConnectAsync(Socket socket, IPEndPoint remoteEndpoint, CancellationToken cancellationToken)
        {
            await socket.ConnectAsync(remoteEndpoint, cancellationToken).ConfigureAwait(false);
        }

        private static void ConnectCore(Socket socket, IPEndPoint remoteEndpoint, TimeSpan connectTimeout, bool ownsSocket)
        {
#if FEATURE_SOCKET_EAP
            var connectCompleted = new ManualResetEvent(false);
            var args = new SocketAsyncEventArgs
                {
                    UserToken = connectCompleted,
                    RemoteEndPoint = remoteEndpoint
                };
            args.Completed += ConnectCompleted;

            if (socket.ConnectAsync(args))
            {
                if (!connectCompleted.WaitOne(connectTimeout))
                {
                    // avoid ObjectDisposedException in ConnectCompleted
                    args.Completed -= ConnectCompleted;
                    if (ownsSocket)
                    {
                        // dispose Socket
                        socket.Dispose();
                    }
                    // dispose ManualResetEvent
                    connectCompleted.Dispose();
                    // dispose SocketAsyncEventArgs
                    args.Dispose();

                    throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                                                                         "Connection failed to establish within {0:F0} milliseconds.",
                                                                         connectTimeout.TotalMilliseconds));
                }
            }

            // dispose ManualResetEvent
            connectCompleted.Dispose();

            if (args.SocketError != SocketError.Success)
            {
                var socketError = (int) args.SocketError;

                if (ownsSocket)
                {
                    // dispose Socket
                    socket.Dispose();
                }

                // dispose SocketAsyncEventArgs
                args.Dispose();

                throw new SocketException(socketError);
            }

            // dispose SocketAsyncEventArgs
            args.Dispose();
#elif FEATURE_SOCKET_APM
            var connectResult = socket.BeginConnect(remoteEndpoint, null, null);
            if (!connectResult.AsyncWaitHandle.WaitOne(connectTimeout, false))
                throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                    "Connection failed to establish within {0:F0} milliseconds.", connectTimeout.TotalMilliseconds));
            socket.EndConnect(connectResult);
#elif FEATURE_SOCKET_TAP
            if (!socket.ConnectAsync(remoteEndpoint).Wait(connectTimeout))
                throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                    "Connection failed to establish within {0:F0} milliseconds.", connectTimeout.TotalMilliseconds));
#else
            #error Connecting to a remote endpoint is not implemented.
#endif
        }

        public static void ClearReadBuffer(Socket socket)
        {
            var timeout = TimeSpan.FromMilliseconds(500);
            var buffer = new byte[256];
            int bytesReceived;

            do
            {
                bytesReceived = ReadPartial(socket, buffer, 0, buffer.Length, timeout);
            }
            while (bytesReceived > 0);
        }

        public static int ReadPartial(Socket socket, byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            socket.ReceiveTimeout = (int) timeout.TotalMilliseconds;

            try
            {
                return socket.Receive(buffer, offset, size, SocketFlags.None);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut)
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                        "Socket read operation has timed out after {0:F0} milliseconds.", timeout.TotalMilliseconds));
                throw;
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
                        break;

                    processReceivedBytesAction(buffer, offset, bytesRead);
                }
                catch (SocketException ex)
                {
                    if (IsErrorResumable(ex.SocketErrorCode))
                        continue;

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
                return -1;

            return buffer[0];
        }

        /// <summary>
        /// Sends a byte using the specified <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to write to.</param>
        /// <param name="value">The value to send.</param>
        /// <exception cref="SocketException">The write failed.</exception>
        public static void SendByte(Socket socket, byte value)
        {
            var buffer = new[] {value};
            Send(socket, buffer, 0, 1);
        }

        /// <summary>
        /// Receives data from a bound <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="timeout">Specifies the amount of time after which the call will time out.</param>
        /// <returns>
        /// The bytes received.
        /// </returns>
        /// <remarks>
        /// If no data is available for reading, the <see cref="Read(Socket, int, TimeSpan)"/> method will
        /// block until data is available or the time-out value is exceeded. If the time-out value is exceeded, the
        /// <see cref="Read(Socket, int, TimeSpan)"/> call will throw a <see cref="SshOperationTimeoutException"/>.
        ///  If you are in non-blocking mode, and there is no data available in the in the protocol stack buffer, the
        /// <see cref="Read(Socket, int, TimeSpan)"/> method will complete immediately and throw a <see cref="SocketException"/>.
        /// </remarks>
        public static byte[] Read(Socket socket, int size, TimeSpan timeout)
        {
            var buffer = new byte[size];
            Read(socket, buffer, 0, size, timeout);
            return buffer;
        }

        public static Task<int> ReadAsync(Socket socket, byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            return socket.ReceiveAsync(buffer, offset, length, cancellationToken);
        }

        /// <summary>
        /// Receives data from a bound <see cref="Socket"/> into a receive buffer.
        /// </summary>
        /// <param name="socket"></param>
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
        /// <para>
        /// If you are in non-blocking mode, and there is no data available in the in the protocol stack buffer, the
        /// <see cref="Read(Socket, byte[], int, int, TimeSpan)"/> method will complete immediately and throw a <see cref="SocketException"/>.
        /// </para>
        /// </remarks>
        public static int Read(Socket socket, byte[] buffer, int offset, int size, TimeSpan readTimeout)
        {
            var totalBytesRead = 0;
            var totalBytesToRead = size;

            socket.ReceiveTimeout = (int)readTimeout.TotalMilliseconds;

            do
            {
                try
                {
                    var bytesRead = socket.Receive(buffer, offset + totalBytesRead, totalBytesToRead - totalBytesRead, SocketFlags.None);
                    if (bytesRead == 0)
                        return 0;

                    totalBytesRead += bytesRead;
                }
                catch (SocketException ex)
                {
                    if (IsErrorResumable(ex.SocketErrorCode))
                    {
                        ThreadAbstraction.Sleep(30);
                        continue;
                    }

                    if (ex.SocketErrorCode == SocketError.TimedOut)
                        throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                            "Socket read operation has timed out after {0:F0} milliseconds.", readTimeout.TotalMilliseconds));

                    throw;
                }
            }
            while (totalBytesRead < totalBytesToRead);

            return totalBytesRead;
        }

        public static void Send(Socket socket, byte[] data)
        {
            Send(socket, data, 0, data.Length);
        }

        public static void Send(Socket socket, byte[] data, int offset, int size)
        {
            var totalBytesSent = 0;  // how many bytes are already sent
            var totalBytesToSend = size;

            do
            {
                try
                {
                    var bytesSent = socket.Send(data, offset + totalBytesSent, totalBytesToSend - totalBytesSent, SocketFlags.None);
                    if (bytesSent == 0)
                        throw new SshConnectionException("An established connection was aborted by the server.",
                                                         DisconnectReason.ConnectionLost);

                    totalBytesSent += bytesSent;
                }
                catch (SocketException ex)
                {
                    if (IsErrorResumable(ex.SocketErrorCode))
                    {
                        // socket buffer is probably full, wait and try again
                        ThreadAbstraction.Sleep(30);
                    }
                    else
                        throw;  // any serious error occurr
                }
            } while (totalBytesSent < totalBytesToSend);
        }

        public static bool IsErrorResumable(SocketError socketError)
        {
            switch (socketError)
            {
                case SocketError.WouldBlock:
                case SocketError.IOPending:
                case SocketError.NoBufferSpaceAvailable:
                    return true;
                default:
                    return false;
            }
        }

#if FEATURE_SOCKET_EAP
        private static void ConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            var eventWaitHandle = (ManualResetEvent) e.UserToken;
            if (eventWaitHandle != null)
                eventWaitHandle.Set();
        }
#endif // FEATURE_SOCKET_EAP

    }
}
