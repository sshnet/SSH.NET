using System.Globalization;
using System.Linq;
using System;
using System.Net.Sockets;
using System.Net;
using Renci.SshNet.Common;
using System.Threading;
using Renci.SshNet.Messages.Transport;
using System.Diagnostics;
using System.Collections.Generic;

namespace Renci.SshNet
{
    public partial class Session
    {
        private const byte Null = 0x00;
        private const byte CarriageReturn = 0x0d;
        private const byte LineFeed = 0x0a;

        private readonly TraceSource _log =
#if DEBUG
            new TraceSource("SshNet.Logging", SourceLevels.All);
#else
            new TraceSource("SshNet.Logging");
#endif

        /// <summary>
        /// Holds the lock object to ensure read access to the socket is synchronized.
        /// </summary>
        private readonly object _socketReadLock = new object();

        /// <summary>
        /// Gets a value indicating whether the socket is connected.
        /// </summary>
        /// <param name="isConnected"><c>true</c> if the socket is connected; otherwise, <c>false</c></param>
        /// <remarks>
        /// <para>
        /// As a first check we verify whether <see cref="Socket.Connected"/> is
        /// <c>true</c>. However, this only returns the state of the socket as of
        /// the last I/O operation. Therefore we use the combination of Socket.Poll
        /// with mode SelectRead and Socket.Available to verify if the socket is
        /// still connected.
        /// </para>
        /// <para>
        /// The MSDN doc mention the following on the return value of <see cref="Socket.Poll(int, SelectMode)"/>
        /// with mode <see cref="SelectMode.SelectRead"/>:
        /// <list type="bullet">
        ///     <item>
        ///         <description><c>true</c> if data is available for reading;</description>
        ///     </item>
        ///     <item>
        ///         <description><c>true</c> if the connection has been closed, reset, or terminated; otherwise, returns <c>false</c>.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// <c>Conclusion:</c> when the return value is <c>true</c> - but no data is available for reading - then
        /// the socket is no longer connected.
        /// </para>
        /// <para>
        /// When a <see cref="Socket"/> is used from multiple threads, there's a race condition
        /// between the invocation of <see cref="Socket.Poll(int, SelectMode)"/> and the moment
        /// when the value of <see cref="Socket.Available"/> is obtained. As a workaround, we signal
        /// when bytes are read from the <see cref="Socket"/>.
        /// </para>
        /// </remarks>
        partial void IsSocketConnected(ref bool isConnected)
        {
            isConnected = (_socket != null && _socket.Connected);
            if (isConnected)
            {
                // synchronize this to ensure thread B does not reset the wait handle before
                // thread A was able to check whether "bytes read from socket" signal was
                // actually received
                lock (_socketReadLock)
                {
                    _bytesReadFromSocket.Reset();
                    var connectionClosedOrDataAvailable = _socket.Poll(1000, SelectMode.SelectRead);
                    isConnected = !(connectionClosedOrDataAvailable && _socket.Available == 0);
                    if (!isConnected)
                    {
                        // the race condition is between the Socket.Poll call and
                        // Socket.Available, but the event handler - where we signal that
                        // bytes have been received from the socket - is sometimes invoked
                        // shortly after
                        isConnected = _bytesReadFromSocket.WaitOne(500);
                    }
                }
            }
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
            const int socketBufferSize = 2 * MaximumSshPacketSize;

            var ipAddress = host.GetIPAddress();
            var timeout = ConnectionInfo.Timeout;
            var ep = new IPEndPoint(ipAddress, port);

            _socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, socketBufferSize);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, socketBufferSize);

            Log(string.Format("Initiating connect to '{0}:{1}'.", ConnectionInfo.Host, ConnectionInfo.Port));

            var connectResult = _socket.BeginConnect(ep, null, null);
            if (!connectResult.AsyncWaitHandle.WaitOne(timeout, false))
                throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                    "Connection failed to establish within {0:F0} milliseconds.", timeout.TotalMilliseconds));

            _socket.EndConnect(connectResult);
        }

        /// <summary>
        /// Closes the socket and allows the socket to be reused after the current connection is closed.
        /// </summary>
        /// <exception cref="SocketException">An error occurred when trying to access the socket.</exception>
        partial void SocketDisconnect()
        {
            _socket.Disconnect(true);
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
                var asyncResult = _socket.BeginReceive(data, 0, data.Length, SocketFlags.None, null, null);
                if (!asyncResult.AsyncWaitHandle.WaitOne(timeout))
                    throw new SshOperationTimeoutException(string.Format(CultureInfo.InvariantCulture,
                        "Socket read operation has timed out after {0:F0} milliseconds.", timeout.TotalMilliseconds));

                var received = _socket.EndReceive(asyncResult);

                if (received == 0)
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
                response = encoding.GetString(buffer.Take(buffer.Count - 2).ToArray());
            else if (buffer.Count > 1 && buffer[buffer.Count - 1] == LineFeed)
                // strip trailing LF
                response = encoding.GetString(buffer.Take(buffer.Count - 1).ToArray());
            else
                response = encoding.GetString(buffer.ToArray());
        }

        /// <summary>
        /// Performs a blocking read on the socket until <paramref name="length"/> bytes are received.
        /// </summary>
        /// <param name="length">The number of bytes to read.</param>
        /// <param name="buffer">The buffer to read to.</param>
        /// <exception cref="SshConnectionException">The socket is closed.</exception>
        /// <exception cref="SocketException">The read failed.</exception>
        partial void SocketRead(int length, ref byte[] buffer)
        {
            var receivedTotal = 0;  // how many bytes is already received

            do
            {
                try
                {
                    var receivedBytes = _socket.Receive(buffer, receivedTotal, length - receivedTotal, SocketFlags.None);
                    if (receivedBytes > 0)
                    {
                        // signal that bytes have been read from the socket
                        // this is used to improve accuracy of Session.IsSocketConnected
                        _bytesReadFromSocket.Set();
                        receivedTotal += receivedBytes;
                        continue;
                    }

                    // 2012-09-11: Kenneth_aa
                    // When Disconnect or Dispose is called, this throws SshConnectionException(), which...
                    // 1 - goes up to ReceiveMessage() 
                    // 2 - up again to MessageListener()
                    // which is where there is a catch-all exception block so it can notify event listeners.
                    // 3 - MessageListener then again calls RaiseError().
                    // There the exception is checked for the exception thrown here (ConnectionLost), and if it matches it will not call Session.SendDisconnect().
                    //
                    // Adding a check for _isDisconnecting causes ReceiveMessage() to throw SshConnectionException: "Bad packet length {0}".
                    //

                    if (_isDisconnecting)
                        throw new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReason.ConnectionLost);
                    throw new SshConnectionException("An established connection was aborted by the server.", DisconnectReason.ConnectionLost);
                }
                catch (SocketException exp)
                {
                    if (exp.SocketErrorCode == SocketError.ConnectionAborted)
                    {
                        buffer = new byte[length];
                        Disconnect();
                        return;
                    }

                    if (exp.SocketErrorCode == SocketError.WouldBlock ||
                        exp.SocketErrorCode == SocketError.IOPending ||
                        exp.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw;  // any serious error occurred
                }
            } while (receivedTotal < length);
        }

        /// <summary>
        /// Writes the specified data to the server.
        /// </summary>
        /// <param name="data">The data to write to the server.</param>
        /// <exception cref="SshOperationTimeoutException">The write has timed-out.</exception>
        /// <exception cref="SocketException">The write failed.</exception>
        partial void SocketWrite(byte[] data)
        {
            var totalBytesSent = 0;  // how many bytes are already sent
            var totalBytesToSend = data.Length;

            do
            {
                try
                {
                    totalBytesSent += _socket.Send(data, totalBytesSent, totalBytesToSend - totalBytesSent,
                        SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw;  // any serious error occurr
                }
            } while (totalBytesSent < totalBytesToSend);
        }

        [Conditional("DEBUG")]
        partial void Log(string text)
        {
            _log.TraceEvent(TraceEventType.Verbose, 1, text);
        }

#if ASYNC_SOCKET_READ
        private void SocketRead(int length, ref byte[] buffer)
        {
            var state = new SocketReadState(_socket, length, ref buffer);

            _socket.BeginReceive(buffer, 0, length, SocketFlags.None, SocketReceiveCallback, state);

            var readResult = state.Wait();
            switch (readResult)
            {
                case SocketReadResult.Complete:
                    break;
                case SocketReadResult.ConnectionLost:
                    if (_isDisconnecting)
                        throw new SshConnectionException(
                            "An established connection was aborted by the software in your host machine.",
                            DisconnectReason.ConnectionLost);
                    throw new SshConnectionException("An established connection was aborted by the server.",
                        DisconnectReason.ConnectionLost);
                case SocketReadResult.Failed:
                    var socketException = state.Exception as SocketException;
                    if (socketException != null)
                    {
                        if (socketException.SocketErrorCode == SocketError.ConnectionAborted)
                        {
                            buffer = new byte[length];
                            Disconnect();
                            return;
                        }
                    }
                    throw state.Exception;
            }
        }

        private void SocketReceiveCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as SocketReadState;
            var socket = state.Socket;

            try
            {
                var bytesReceived = socket.EndReceive(ar);
                if (bytesReceived > 0)
                {
                    _bytesReadFromSocket.Set();
                    state.BytesRead += bytesReceived;
                    if (state.BytesRead < state.TotalBytesToRead)
                    {
                        socket.BeginReceive(state.Buffer, state.BytesRead, state.TotalBytesToRead - state.BytesRead,
                            SocketFlags.None, SocketReceiveCallback, state);
                    }
                    else
                    {
                        // we received all bytes that we wanted, so lets mark the read
                        // complete
                        state.Complete();
                    }
                }
                else
                {
                    // the remote host shut down the connection; this could also have been
                    // triggered by a SSH_MSG_DISCONNECT sent by the client
                    state.ConnectionLost();
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.ConnectionAborted)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);

                        socket.BeginReceive(state.Buffer, state.BytesRead, state.TotalBytesToRead - state.BytesRead,
                            SocketFlags.None, SocketReceiveCallback, state);
                        return;
                    }
                }

                state.Fail(ex);
            }
            catch (Exception ex)
            {
                state.Fail(ex);
            }
        }

        private class SocketReadState
        {
            private SocketReadResult _result;

            /// <summary>
            /// WaitHandle to signal that read from socket has completed (either successfully
            /// or with failure)
            /// </summary>
            private EventWaitHandle _socketReadComplete;

            public SocketReadState(Socket socket, int totalBytesToRead, ref byte[] buffer)
            {
                Socket = socket;
                TotalBytesToRead = totalBytesToRead;
                Buffer = buffer;
                _socketReadComplete = new ManualResetEvent(false);
            }

            /// <summary>
            /// Gets the <see cref="Socket"/> to read from.
            /// </summary>
            /// <value>
            /// The <see cref="Socket"/> to read from.
            /// </value>
            public Socket Socket { get; private set; }

            /// <summary>
            /// Gets or sets the number of bytes that have been read from the <see cref="Socket"/>.
            /// </summary>
            /// <value>
            /// The number of bytes that have been read from the <see cref="Socket"/>.
            /// </value>
            public int BytesRead { get; set; }

            /// <summary>
            /// Gets the total number of bytes to read from the <see cref="Socket"/>.
            /// </summary>
            /// <value>
            /// The total number of bytes to read from the <see cref="Socket"/>.
            /// </value>
            public int TotalBytesToRead { get; private set; }

            /// <summary>
            /// Gets or sets the buffer to hold the bytes that have been read.
            /// </summary>
            /// <value>
            /// The buffer to hold the bytes that have been read.
            /// </value>
            public byte[] Buffer { get; private set; }

            /// <summary>
            /// Gets or sets the exception that was thrown while reading from the
            /// <see cref="Socket"/>.
            /// </summary>
            /// <value>
            /// The exception that was thrown while reading from the <see cref="Socket"/>,
            /// or <c>null</c> if no exception was thrown.
            /// </value>
            public Exception Exception { get; private set; }

            /// <summary>
            /// Signals that the total number of bytes has been read successfully.
            /// </summary>
            public void Complete()
            {
                _result = SocketReadResult.Complete;
                _socketReadComplete.Set();
            }

            /// <summary>
            /// Signals that the socket read failed.
            /// </summary>
            /// <param name="cause">The <see cref="Exception"/> that caused the read to fail.</param>
            public void Fail(Exception cause)
            {
                Exception = cause;
                _result = SocketReadResult.Failed;
                _socketReadComplete.Set();
            }

            /// <summary>
            /// Signals that the connection to the server was lost.
            /// </summary>
            public void ConnectionLost()
            {
                _result = SocketReadResult.ConnectionLost;
                _socketReadComplete.Set();
            }

            public SocketReadResult Wait()
            {
                _socketReadComplete.WaitOne();
                _socketReadComplete.Dispose();
                _socketReadComplete = null;
                return _result;
            }
        }

        private enum SocketReadResult
        {
            Complete,
            ConnectionLost,
            Failed
        }
#endif
    }
}
