using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

#pragma warning disable MA0204 // Remove unnecessary partial modifier; not true for all targets

namespace Renci.SshNet.Abstractions
{
    internal static partial class SocketAbstraction
    {
        public static Socket Connect(IPEndPoint remoteEndpoint, TimeSpan connectTimeout)
        {
            var socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            ConnectCore(socket, remoteEndpoint, connectTimeout, ownsSocket: true);
            return socket;
        }

        public static void Connect(Socket socket, EndPoint remoteEndpoint, TimeSpan connectTimeout)
        {
            ConnectCore(socket, remoteEndpoint, connectTimeout, ownsSocket: false);
        }

        public static async Task ConnectAsync(Socket socket, EndPoint remoteEndpoint, CancellationToken cancellationToken)
        {
            await socket.ConnectAsync(remoteEndpoint, cancellationToken).ConfigureAwait(false);
        }

        private static void ConnectCore(Socket socket, EndPoint remoteEndpoint, TimeSpan connectTimeout, bool ownsSocket)
        {
            var connectCompleted = new ManualResetEvent(initialState: false);
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
                var socketError = (int)args.SocketError;

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
            // Socket.ReceiveTimeout (SO_RCVTIMEO) is not reliably enforced on Mono-derived runtimes
            // when the TCP handshake completes but the remote peer subsequently stops responding
            // without sending a RST/FIN (no exception is thrown and the blocking Receive call can
            // hang indefinitely). Socket.Poll's timeout is honored via a plain select()/poll() system
            // call, which is far more consistently implemented, so use a poll-based deadline loop
            // instead of depending on the receive timeout socket option.
            if (!PollWithDeadline(socket, timeout, out var _))
            {
                throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                                                                     "Socket read operation has timed out after {0:F0} milliseconds.",
                                                                     timeout.TotalMilliseconds));
            }

            return socket.Receive(buffer, offset, size, SocketFlags.None);
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
                    if (IsErrorResumable(ex.SocketErrorCode))
                    {
                        continue;
                    }

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
        /// Sends a byte using the specified <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to write to.</param>
        /// <param name="value">The value to send.</param>
        /// <exception cref="SocketException">The write failed.</exception>
        public static void SendByte(Socket socket, byte value)
        {
            var buffer = new[] { value };
            Send(socket, buffer, 0, 1);
        }

        /// <summary>
        /// Receives data from a bound <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to read from.</param>
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
            _ = Read(socket, buffer, 0, size, timeout);
            return buffer;
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
        /// <para>
        /// If you are in non-blocking mode, and there is no data available in the in the protocol stack buffer, the
        /// <see cref="Read(Socket, byte[], int, int, TimeSpan)"/> method will complete immediately and throw a <see cref="SocketException"/>.
        /// </para>
        /// </remarks>
        public static int Read(Socket socket, byte[] buffer, int offset, int size, TimeSpan readTimeout)
        {
            var totalBytesRead = 0;
            var totalBytesToRead = size;

            // See the remarks on ReadPartial: rely on Socket.Poll's deadline rather than
            // Socket.ReceiveTimeout, which is not consistently enforced on Mono-derived runtimes
            // when the remote peer goes silent after the TCP handshake completes.
            //
            // Timeout.InfiniteTimeSpan (-1 ms) means "wait forever" and must NOT be added to
            // DateTime.UtcNow: doing so produces a deadline already in the past (UtcNow - 1ms),
            // which makes the very first non-blocking poll below spuriously throw an
            // SshOperationTimeoutException ("...timed out after -1 milliseconds") whenever data
            // is not already available at that exact instant, even though the caller asked to
            // wait indefinitely. Skip deadline tracking entirely in that case and let
            // PollWithDeadline block forever on each iteration.
            var isInfiniteTimeout = readTimeout == Timeout.InfiniteTimeSpan;
            var deadlineUtc = isInfiniteTimeout ? DateTime.MaxValue : DateTime.UtcNow + readTimeout;

            do
            {
                var remaining = isInfiniteTimeout ? Timeout.InfiniteTimeSpan : deadlineUtc - DateTime.UtcNow;
                if (!isInfiniteTimeout && remaining < TimeSpan.Zero)
                {
                    remaining = TimeSpan.Zero;
                }

                if (!PollWithDeadline(socket, remaining, out var _))
                {
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                                                           "Socket read operation has timed out after {0:F0} milliseconds.",
                                                           readTimeout.TotalMilliseconds));
                }

                try
                {
                    var bytesRead = socket.Receive(buffer, offset + totalBytesRead, totalBytesToRead - totalBytesRead, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        return 0;
                    }

                    totalBytesRead += bytesRead;
                }
                catch (SocketException ex)
                {
                    if (IsErrorResumable(ex.SocketErrorCode))
                    {
                        Thread.Sleep(30);
                        continue;
                    }

                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                                                               "Socket read operation has timed out after {0:F0} milliseconds.",
                                                               readTimeout.TotalMilliseconds));
                    }

                    throw;
                }
            }
            while (totalBytesRead < totalBytesToRead);

            return totalBytesRead;
        }

        /// <summary>
        /// Waits, using <see cref="Socket.Poll(int, SelectMode)"/>, until the specified <see cref="Socket"/>
        /// has data available to read or the specified <paramref name="timeout"/> elapses.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to poll.</param>
        /// <param name="timeout">The maximum time to wait for data to become available.</param>
        /// <param name="remaining">The time remaining in <paramref name="timeout"/> when data became available.</param>
        /// <returns>
        /// <see langword="true"/> if data is available to read (or the connection was closed by the
        /// remote host) before <paramref name="timeout"/> elapses; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Unlike <see cref="Socket.ReceiveTimeout"/>, which is enforced differently (and, on some
        /// runtimes, unreliably) across platforms, <see cref="Socket.Poll(int, SelectMode)"/> maps
        /// directly onto the underlying select()/poll() system call and consistently unblocks when
        /// its timeout elapses, even if the remote peer never sends any data and never resets or
        /// closes the connection.
        /// </remarks>
        private static bool PollWithDeadline(Socket socket, TimeSpan timeout, out TimeSpan remaining)
        {
            // Timeout.InfiniteTimeSpan (-1 ms) means "wait forever" and must NOT be added to
            // DateTime.UtcNow below: doing so produces a deadline that is already in the past
            // (UtcNow - 1ms), so the very first non-blocking Socket.Poll(0, ...) call that does not
            // immediately find data available would incorrectly be treated as an expired deadline
            // and spuriously throw an SshOperationTimeoutException ("...timed out after -1
            // milliseconds") even though the caller asked to wait indefinitely. Poll with an actual
            // infinite/blocking wait (-1 microseconds) instead in that case.
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                _ = socket.Poll(-1, SelectMode.SelectRead);
                remaining = Timeout.InfiniteTimeSpan;
                return true;
            }

            var deadlineUtc = DateTime.UtcNow + timeout;

            while (true)
            {
                var timeUntilDeadline = deadlineUtc - DateTime.UtcNow;
                if (timeUntilDeadline < TimeSpan.Zero)
                {
                    timeUntilDeadline = TimeSpan.Zero;
                }

                // Socket.Poll caps its microseconds parameter at int.MaxValue; clamp accordingly
                // instead of overflowing when timeUntilDeadline is very large (e.g. Timeout.InfiniteTimeSpan).
                var microseconds = timeUntilDeadline.TotalMilliseconds * 1000d;
                var pollMicroseconds = microseconds >= int.MaxValue ? int.MaxValue : (int)microseconds;

                if (socket.Poll(pollMicroseconds, SelectMode.SelectRead))
                {
                    remaining = deadlineUtc - DateTime.UtcNow;
                    return true;
                }

                if (DateTime.UtcNow >= deadlineUtc)
                {
                    remaining = TimeSpan.Zero;
                    return false;
                }
            }
        }

#if !NET
        public static Task<int> ReadAsync(Socket socket, byte[] buffer, CancellationToken cancellationToken)
        {
            return socket.ReceiveAsync(buffer, 0, buffer.Length, cancellationToken);
        }
#endif

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
                    {
                        throw new SshConnectionException("An established connection was aborted by the server.",
                                                         DisconnectReason.ConnectionLost);
                    }

                    totalBytesSent += bytesSent;
                }
                catch (SocketException ex)
                {
                    if (IsErrorResumable(ex.SocketErrorCode))
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                    {
                        throw; // any serious error occur
                    }
                }
            }
            while (totalBytesSent < totalBytesToSend);
        }

        public static bool IsErrorResumable(SocketError socketError)
        {
#pragma warning disable IDE0010 // Add missing cases
            switch (socketError)
            {
                case SocketError.WouldBlock:
                case SocketError.IOPending:
                case SocketError.NoBufferSpaceAvailable:
                    return true;
                default:
                    return false;
            }
#pragma warning restore IDE0010 // Add missing cases
        }

        private static void ConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            var eventWaitHandle = (ManualResetEvent)e.UserToken;
            _ = eventWaitHandle?.Set();
        }
    }
}
