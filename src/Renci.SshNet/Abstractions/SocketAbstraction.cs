using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
#if FEATURE_SOCKET_POLL
                return socket.Poll(-1, SelectMode.SelectRead) && socket.Available > 0;
#else
                return true;
#endif // FEATURE_SOCKET_POLL
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
#if FEATURE_SOCKET_POLL
                return socket.Poll(-1, SelectMode.SelectWrite);
#else
                return true;
#endif // FEATURE_SOCKET_POLL
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
#if FEATURE_SOCKET_SYNC
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
#elif FEATURE_SOCKET_EAP
            var receiveCompleted = new ManualResetEvent(false);
            var sendReceiveToken = new PartialSendReceiveToken(socket, receiveCompleted);
            var args = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = socket.RemoteEndPoint,
                    UserToken = sendReceiveToken
                };
            args.Completed += ReceiveCompleted;
            args.SetBuffer(buffer, offset, size);

            try
            {
                if (socket.ReceiveAsync(args))
                {
                    if (!receiveCompleted.WaitOne(timeout))
                        throw new SshOperationTimeoutException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Socket read operation has timed out after {0:F0} milliseconds.",
                                timeout.TotalMilliseconds));
                }
                else
                {
                    sendReceiveToken.Process(args);
                }

                if (args.SocketError != SocketError.Success)
                    throw new SocketException((int) args.SocketError);

                return args.BytesTransferred;
            }
            finally
            {
                // initialize token to avoid the waithandle getting used after it's disposed
                args.UserToken = null;
                args.Dispose();
                receiveCompleted.Dispose();
            }
#else
            #error Receiving data from a Socket is not implemented.
#endif
        }

        public static void ReadContinuous(Socket socket, byte[] buffer, int offset, int size, Action<byte[], int, int> processReceivedBytesAction)
        {
#if FEATURE_SOCKET_SYNC
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
#elif FEATURE_SOCKET_EAP
            var completionWaitHandle = new ManualResetEvent(false);
            var readToken = new ContinuousReceiveToken(socket, processReceivedBytesAction, completionWaitHandle);
            var args = new SocketAsyncEventArgs
            {
                RemoteEndPoint = socket.RemoteEndPoint,
                UserToken = readToken
            };
            args.Completed += ReceiveCompleted;
            args.SetBuffer(buffer, offset, size);

            if (!socket.ReceiveAsync(args))
            {
                ReceiveCompleted(null, args);
            }

            completionWaitHandle.WaitOne();
            completionWaitHandle.Dispose();

            if (readToken.Exception != null)
                throw readToken.Exception;
#else
            #error Receiving data from a Socket is not implemented.
#endif
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
#if FEATURE_SOCKET_SYNC
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
#elif FEATURE_SOCKET_EAP
            var receiveCompleted = new ManualResetEvent(false);
            var sendReceiveToken = new BlockingSendReceiveToken(socket, buffer, offset, size, receiveCompleted);

            var args = new SocketAsyncEventArgs
                {
                    UserToken = sendReceiveToken,
                    RemoteEndPoint = socket.RemoteEndPoint
                };
            args.Completed += ReceiveCompleted;
            args.SetBuffer(buffer, offset, size);

            try
            {
                if (socket.ReceiveAsync(args))
                {
                    if (!receiveCompleted.WaitOne(readTimeout))
                        throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                            "Socket read operation has timed out after {0:F0} milliseconds.", readTimeout.TotalMilliseconds));
                }
                else
                {
                    sendReceiveToken.Process(args);
                }

                if (args.SocketError != SocketError.Success)
                        throw new SocketException((int) args.SocketError);

                return sendReceiveToken.TotalBytesTransferred;
            }
            finally
            {
                // initialize token to avoid the waithandle getting used after it's disposed
                args.UserToken = null;
                args.Dispose();
                receiveCompleted.Dispose();
            }
#else
#error Receiving data from a Socket is not implemented.
#endif
        }

        public static void Send(Socket socket, byte[] data)
        {
            Send(socket, data, 0, data.Length);
        }

        public static void Send(Socket socket, byte[] data, int offset, int size)
        {
#if FEATURE_SOCKET_SYNC
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
#elif FEATURE_SOCKET_EAP
            var sendCompleted = new ManualResetEvent(false);
            var sendReceiveToken = new BlockingSendReceiveToken(socket, data, offset, size, sendCompleted);
            var socketAsyncSendArgs = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = socket.RemoteEndPoint,
                    UserToken = sendReceiveToken
                };
            socketAsyncSendArgs.SetBuffer(data, offset, size);
            socketAsyncSendArgs.Completed += SendCompleted;

            try
            {
                if (socket.SendAsync(socketAsyncSendArgs))
                {
                    if (!sendCompleted.WaitOne())
                        throw new SocketException((int) SocketError.TimedOut);
                }
                else
                {
                    sendReceiveToken.Process(socketAsyncSendArgs);
                }

                if (socketAsyncSendArgs.SocketError != SocketError.Success)
                    throw new SocketException((int) socketAsyncSendArgs.SocketError);

                if (sendReceiveToken.TotalBytesTransferred == 0)
                    throw new SshConnectionException("An established connection was aborted by the server.",
                                                     DisconnectReason.ConnectionLost);
            }
            finally
            {
                // initialize token to avoid the completion waithandle getting used after it's disposed
                socketAsyncSendArgs.UserToken = null;
                socketAsyncSendArgs.Dispose();
                sendCompleted.Dispose();
            }
#else
            #error Sending data to a Socket is not implemented.
#endif
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

#if FEATURE_SOCKET_EAP && !FEATURE_SOCKET_SYNC
        private static void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            var sendReceiveToken = (Token) e.UserToken;
            if (sendReceiveToken != null)
                sendReceiveToken.Process(e);
        }

        private static void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            var sendReceiveToken = (Token) e.UserToken;
            if (sendReceiveToken != null)
                sendReceiveToken.Process(e);
        }

        private interface Token
        {
            void Process(SocketAsyncEventArgs args);
        }

        private class BlockingSendReceiveToken : Token
        {
            public BlockingSendReceiveToken(Socket socket, byte[] buffer, int offset, int size, EventWaitHandle completionWaitHandle)
            {
                _socket = socket;
                _buffer = buffer;
                _offset = offset;
                _bytesToTransfer = size;
                _completionWaitHandle = completionWaitHandle;
            }

            public void Process(SocketAsyncEventArgs args)
            {
                if (args.SocketError == SocketError.Success)
                {
                    TotalBytesTransferred += args.BytesTransferred;

                    if (TotalBytesTransferred == _bytesToTransfer)
                    {
                        // finished transferring specified bytes
                        _completionWaitHandle.Set();
                        return;
                    }

                    if (args.BytesTransferred == 0)
                    {
                        // remote server closed the connection
                        _completionWaitHandle.Set();
                        return;
                    }

                    _offset += args.BytesTransferred;
                    args.SetBuffer(_buffer, _offset, _bytesToTransfer - TotalBytesTransferred);
                    ResumeOperation(args);
                    return;
                }

                if (IsErrorResumable(args.SocketError))
                {
                    ThreadAbstraction.Sleep(30);
                    ResumeOperation(args);
                    return;
                }

                // we're dealing with a (fatal) error
                _completionWaitHandle.Set();
            }

            private void ResumeOperation(SocketAsyncEventArgs args)
            {
                switch (args.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        _socket.ReceiveAsync(args);
                        break;
                    case SocketAsyncOperation.Send:
                        _socket.SendAsync(args);
                        break;
                }
            }

            private readonly int _bytesToTransfer;
            public int TotalBytesTransferred { get; private set; }
            private readonly EventWaitHandle _completionWaitHandle;
            private readonly Socket _socket;
            private readonly byte[] _buffer;
            private int _offset;
        }

        private class PartialSendReceiveToken : Token
        {
            public PartialSendReceiveToken(Socket socket, EventWaitHandle completionWaitHandle)
            {
                _socket = socket;
                _completionWaitHandle = completionWaitHandle;
            }

            public void Process(SocketAsyncEventArgs args)
            {
                if (args.SocketError == SocketError.Success)
                {
                    _completionWaitHandle.Set();
                    return;
                }

                if (IsErrorResumable(args.SocketError))
                {
                    ThreadAbstraction.Sleep(30);
                    ResumeOperation(args);
                    return;
                }

                // we're dealing with a (fatal) error
                _completionWaitHandle.Set();
            }

            private void ResumeOperation(SocketAsyncEventArgs args)
            {
                switch (args.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        _socket.ReceiveAsync(args);
                        break;
                    case SocketAsyncOperation.Send:
                        _socket.SendAsync(args);
                        break;
                }
            }

            private readonly EventWaitHandle _completionWaitHandle;
            private readonly Socket _socket;
        }

        private class ContinuousReceiveToken : Token
        {
            public ContinuousReceiveToken(Socket socket, Action<byte[], int, int> processReceivedBytesAction, EventWaitHandle completionWaitHandle)
            {
                _socket = socket;
                _processReceivedBytesAction = processReceivedBytesAction;
                _completionWaitHandle = completionWaitHandle;
            }

            public Exception Exception { get; private set; }

            public void Process(SocketAsyncEventArgs args)
            {
                if (args.SocketError == SocketError.Success)
                {
                    if (args.BytesTransferred == 0)
                    {
                        // remote socket was closed
                        _completionWaitHandle.Set();
                        return;
                    }

                    _processReceivedBytesAction(args.Buffer, args.Offset, args.BytesTransferred);
                    ResumeOperation(args);
                    return;
                }

                if (IsErrorResumable(args.SocketError))
                {
                    ThreadAbstraction.Sleep(30);
                    ResumeOperation(args);
                    return;
                }

                if (args.SocketError != SocketError.OperationAborted)
                {
                    Exception = new SocketException((int) args.SocketError);
                }

                // we're dealing with a (fatal) error
                _completionWaitHandle.Set();
            }

            private void ResumeOperation(SocketAsyncEventArgs args)
            {
                switch (args.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        _socket.ReceiveAsync(args);
                        break;
                    case SocketAsyncOperation.Send:
                        _socket.SendAsync(args);
                        break;
                }
            }

            private readonly EventWaitHandle _completionWaitHandle;
            private readonly Socket _socket;
            private readonly Action<byte[], int, int> _processReceivedBytesAction;
        }
#endif // FEATURE_SOCKET_EAP && !FEATURE_SOCKET_SYNC
    }
}
