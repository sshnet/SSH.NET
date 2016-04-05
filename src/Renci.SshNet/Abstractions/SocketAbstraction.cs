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
#endif // FEATURE_SOCKET_POLL
            }

            return false;

        }

        public static bool CanWrite(Socket socket)
        {
            if (socket.Connected)
            {
#if FEATURE_SOCKET_POLL
                return socket.Poll(-1, SelectMode.SelectWrite);
#endif // FEATURE_SOCKET_POLL
            }

            return false;
        }

        public static Socket Connect(IPEndPoint remoteEndpoint, TimeSpan connectTimeout)
        {
            var socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {NoDelay = true};

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
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                        "Connection failed to establish within {0:F0} milliseconds.", connectTimeout.TotalMilliseconds));
            }

            if (args.SocketError != SocketError.Success)
                throw new SocketException((int) args.SocketError);
            return socket;
#elif FEATURE_SOCKET_APM
            var connectResult = socket.BeginConnect(remoteEndpoint, null, null);
            if (!connectResult.AsyncWaitHandle.WaitOne(connectTimeout, false))
                throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                    "Connection failed to establish within {0:F0} milliseconds.", connectTimeout.TotalMilliseconds));
            socket.EndConnect(connectResult);
            return socket;
#elif FEATURE_SOCKET_TAP
            if (!socket.ConnectAsync(remoteEndpoint).Wait(connectTimeout))
                throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                    "Connection failed to establish within {0:F0} milliseconds.", connectTimeout.TotalMilliseconds));
            return socket;
#else
            #error Connecting to a remote endpoint is not implemented.
#endif
        }

        public static void ClearReadBuffer(Socket socket)
        {
            try
            {
                var buffer = new byte[256];
                int bytesReceived;

                do
                {
                    bytesReceived = ReadPartial(socket, buffer, 0, buffer.Length, TimeSpan.FromSeconds(2));
                } while (bytesReceived > 0);
            }
            catch
            {
                // ignore any exceptions
            }
        }

        public static int ReadPartial(Socket socket, byte[] buffer, int offset, int size, TimeSpan timeout)
        {
#if FEATURE_SOCKET_SYNC
            return socket.Receive(buffer, offset, size, SocketFlags.None);
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

        /// <summary>
        /// Receives data from a bound <see cref="Socket"/>into a receive buffer.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the storage location for the received data. </param>
        /// <param name="offset">The position in <paramref name="buffer"/> parameter to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="timeout">Specifies the amount of time after which the call will time out.</param>
        /// <returns>
        /// The number of bytes received.
        /// </returns>
        /// <remarks>
        /// If no data is available for reading, the <see cref="Read(Socket,byte[], int, int, TimeSpan)"/> method will
        /// block until data is available or the time-out value was exceeded. If the time-out value was exceeded, the
        /// <see cref="Read(Socket,byte[], int, int, TimeSpan)"/> call will throw a <see cref="SshOperationTimeoutException"/>.
        ///  If you are in non-blocking mode, and there is no data available in the in the protocol stack buffer, the
        /// <see cref="Read(Socket,byte[], int, int, TimeSpan)"/> method will complete immediately and throw a <see cref="SocketException"/>.
        /// </remarks>
        public static int Read(Socket socket, byte[] buffer, int offset, int size, TimeSpan timeout)
        {
#if FEATURE_SOCKET_SYNC
            var totalBytesRead = 0;
            var totalBytesToRead = size;

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
                    if (!receiveCompleted.WaitOne(timeout))
                        throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                            "Socket read operation has timed out after {0:F0} milliseconds.", timeout.TotalMilliseconds));
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
                    var bytesSent = socket.Send(data, offset + totalBytesSent, totalBytesToSend - totalBytesSent,
                        SocketFlags.None);
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

        private static bool IsErrorResumable(SocketError socketError)
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
#endif // FEATURE_SOCKET_EAP && !FEATURE_SOCKET_SYNC
    }
}
