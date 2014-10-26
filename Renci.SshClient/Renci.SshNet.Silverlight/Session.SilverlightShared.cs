using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet
{
    public partial class Session
    {
        private const byte Null = 0x00;
        private const byte CarriageReturn = 0x0d;
        private const byte LineFeed = 0x0a;

        private readonly AutoResetEvent _connectEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _sendEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _receiveEvent = new AutoResetEvent(false);

        /// <summary>
        /// Gets a value indicating whether the socket is connected.
        /// </summary>
        /// <param name="isConnected"><c>true</c> if the socket is connected; otherwise, <c>false</c></param>
        partial void IsSocketConnected(ref bool isConnected)
        {
            isConnected = (_socket != null && _socket.Connected);
        }

        /// <summary>
        /// Establishes a socket connection to the specified host and port.
        /// </summary>
        /// <param name="host">The host name of the server to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <exception cref="SshOperationTimeoutException">The connection failed to establish within the configured <see cref="Renci.SshNet.ConnectionInfo.Timeout"/>.</exception>
        /// <exception cref="SocketException">An error occurred trying to establish the connection.</exception>
        partial void SocketConnect(string host, int port)
        {
            var timeout = ConnectionInfo.Timeout;
            var ipAddress = host.GetIPAddress();
            var ep = new IPEndPoint(ipAddress, port);

            _socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var args = CreateSocketAsyncEventArgs(_connectEvent);
            if (_socket.ConnectAsync(args))
            {
                if (!_connectEvent.WaitOne(timeout))
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                        "Connection failed to establish within {0:F0} milliseconds.", timeout.TotalMilliseconds));
            }

            if (args.SocketError != SocketError.Success)
                throw new SocketException((int) args.SocketError);
        }

        /// <summary>
        /// Closes the socket.
        /// </summary>
        /// <remarks>
        /// This method will wait up to <c>10</c> seconds to send any remaining data.
        /// </remarks>
        partial void SocketDisconnect()
        {
            _socket.Close(10);
        }

        /// <summary>
        /// Performs a blocking read on the socket until a line is read.
        /// </summary>
        /// <param name="response">The line read from the socket, or <c>null</c> when the remote server has shutdown and all data has been received.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the time to wait until a line is read.</param>
        /// <exception cref="SshOperationTimeoutException">The read has timed-out.</exception>
        /// <exception cref="SocketException">An error occurred when trying to access the socket.</exception>
        partial void SocketReadLine(ref string response, TimeSpan timeout)
        {
            var encoding = new ASCIIEncoding();
            var buffer = new List<byte>();
            var data = new byte[1];

            // read data one byte at a time to find end of line and leave any unhandled information in the buffer
            // to be processed by subsequent invocations
            do
            {
                var args = CreateSocketAsyncEventArgs(_receiveEvent, data, 0, data.Length);
                if (_socket.ReceiveAsync(args))
                {
                    if (!_receiveEvent.WaitOne(timeout))
                        throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                            "Socket read operation has timed out after {0:F0} milliseconds.", timeout.TotalMilliseconds));
                }

                if (args.SocketError != SocketError.Success)
                    throw new SocketException((int) args.SocketError);

                if (args.BytesTransferred == 0)
                    // the remote server shut down the socket
                    break;

                buffer.Add(data[0]);
            }
            while (!(buffer.Count > 0 && (buffer[buffer.Count - 1] == LineFeed || buffer[buffer.Count - 1] == Null)));

            if (buffer.Count == 0)
                response = null;
            else if (buffer.Count == 1 && buffer[buffer.Count - 1] == 0x00)
                // return an empty version string if the buffer consists of only a 0x00 character
                response = string.Empty;
            else if (buffer.Count > 1 && buffer[buffer.Count - 2] == CarriageReturn)
                // strip trailing CRLF
                response = encoding.GetString(buffer.ToArray(), 0, buffer.Count - 2);
            else if (buffer.Count > 1 && buffer[buffer.Count - 1] == LineFeed)
                // strip trailing LF
                response = encoding.GetString(buffer.ToArray(), 0, buffer.Count - 1);
            else
                response = encoding.GetString(buffer.ToArray(), 0, buffer.Count);
        }

        /// <summary>
        /// Performs a blocking read on the socket until <paramref name="length"/> bytes are received.
        /// </summary>
        /// <param name="length">The number of bytes to read.</param>
        /// <param name="buffer">The buffer to read to.</param>
        /// <exception cref="SshConnectionException">The socket is closed.</exception>
        /// <exception cref="SshOperationTimeoutException">The read has timed-out.</exception>
        /// <exception cref="SocketException">The read failed.</exception>
        partial void SocketRead(int length, ref byte[] buffer)
        {
            var timeout = Infinite;
            var totalBytesReceived = 0;  // how many bytes are already received

            do
            {
                var args = CreateSocketAsyncEventArgs(_receiveEvent, buffer, totalBytesReceived,
                    length - totalBytesReceived);
                if (_socket.ReceiveAsync(args))
                {
                    if (!_receiveEvent.WaitOne(timeout))
                        // currently we wait indefinitely, so this exception will never be thrown
                        // but let's leave this here anyway as we may revisit this later
                        throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                            "Socket read operation has timed out after {0:F0} milliseconds.", timeout.TotalMilliseconds));
                }

                switch (args.SocketError)
                {
                    case SocketError.WouldBlock:
                    case SocketError.IOPending:
                    case SocketError.NoBufferSpaceAvailable:
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                        break;
                    case SocketError.Success:
                        var bytesReceived = args.BytesTransferred;
                        if (bytesReceived > 0)
                        {
                            totalBytesReceived += bytesReceived;
                            continue;
                        }

                        if (_isDisconnecting)
                            throw new SshConnectionException(
                                "An established connection was aborted by the software in your host machine.",
                                DisconnectReason.ConnectionLost);
                        throw new SshConnectionException("An established connection was aborted by the server.",
                            DisconnectReason.ConnectionLost);
                    default:
                        throw new SocketException((int) args.SocketError);
                }
            } while (totalBytesReceived < length);
        }

        /// <summary>
        /// Writes the specified data to the server.
        /// </summary>
        /// <param name="data">The data to write to the server.</param>
        /// <exception cref="SshOperationTimeoutException">The write has timed-out.</exception>
        /// <exception cref="SocketException">The write failed.</exception>
        partial void SocketWrite(byte[] data)
        {
            var timeout = ConnectionInfo.Timeout;
            var totalBytesSent = 0;  // how many bytes are already sent
            var totalBytesToSend = data.Length;

            do
            {
                var args = CreateSocketAsyncEventArgs(_sendEvent, data, 0, totalBytesToSend - totalBytesSent);
                if (_socket.SendAsync(args))
                {
                    if (!_sendEvent.WaitOne(timeout))
                        throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                            "Socket write operation has timed out after {0:F0} milliseconds.", timeout.TotalMilliseconds));
                }

                switch (args.SocketError)
                {
                    case SocketError.WouldBlock:
                    case SocketError.IOPending:
                    case SocketError.NoBufferSpaceAvailable:
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                        break;
                    case SocketError.Success:
                        totalBytesSent += args.BytesTransferred;
                        break;
                    default:
                        throw new SocketException((int) args.SocketError);
}
                } while (totalBytesSent < totalBytesToSend);
        }

        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem(o => action());
        }

        partial void InternalRegisterMessage(string messageName)
        {
            lock (_messagesMetadata)
            {
                foreach (var item in from m in _messagesMetadata where m.Name == messageName select m)
                {
                    item.Enabled = true;
                    item.Activated = true;
                }
            }
        }

        partial void InternalUnRegisterMessage(string messageName)
        {
            lock (_messagesMetadata)
            {
                foreach (var item in from m in _messagesMetadata where m.Name == messageName select m)
                {
                    item.Enabled = false;
                    item.Activated = false;
                }
            }
        }

        private SocketAsyncEventArgs CreateSocketAsyncEventArgs(EventWaitHandle waitHandle)
        {
            var args = new SocketAsyncEventArgs();
            args.UserToken = _socket;
            args.RemoteEndPoint = _socket.RemoteEndPoint;
            args.Completed += (sender, eventArgs) => waitHandle.Set();
            return args;
        }

        private SocketAsyncEventArgs CreateSocketAsyncEventArgs(EventWaitHandle waitHandle, byte[] data, int offset, int count)
        {
            var args = CreateSocketAsyncEventArgs(waitHandle);
            args.SetBuffer(data, offset, count);
            return args;
        }
    }
}
