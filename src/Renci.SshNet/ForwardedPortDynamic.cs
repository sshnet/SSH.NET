using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for forwarding connections from the client to destination servers via the SSH server,
    /// also known as dynamic port forwarding.
    /// </summary>
    public class ForwardedPortDynamic : ForwardedPort
    {
        private ForwardedPortStatus _status;

        /// <summary>
        /// Holds a value indicating whether the current instance is disposed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the current instance is disposed; otherwise, <see langword="false"/>.
        /// </value>
        private bool _isDisposed;

        /// <summary>
        /// Gets the bound host.
        /// </summary>
        public string BoundHost { get; }

        /// <summary>
        /// Gets the bound port.
        /// </summary>
        public uint BoundPort { get; }

        private Socket _listener;
        private CountdownEvent _pendingChannelCountdown;

        /// <summary>
        /// Gets a value indicating whether port forwarding is started.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if port forwarding is started; otherwise, <see langword="false"/>.
        /// </value>
        public override bool IsStarted
        {
            get { return _status == ForwardedPortStatus.Started; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortDynamic"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public ForwardedPortDynamic(uint port)
            : this(string.Empty, port)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardedPortDynamic"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        public ForwardedPortDynamic(string host, uint port)
        {
            BoundHost = host;
            BoundPort = port;
            _status = ForwardedPortStatus.Stopped;
        }

        /// <summary>
        /// Starts local port forwarding.
        /// </summary>
        protected override void StartPort()
        {
            if (!ForwardedPortStatus.ToStarting(ref _status))
            {
                return;
            }

            try
            {
                InternalStart();
            }
            catch (Exception)
            {
                _status = ForwardedPortStatus.Stopped;
                throw;
            }
        }

        /// <summary>
        /// Stops local port forwarding, and waits for the specified timeout until all pending
        /// requests are processed.
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait for pending requests to finish processing.</param>
        protected override void StopPort(TimeSpan timeout)
        {
            timeout.EnsureValidTimeout(nameof(timeout));

            if (!ForwardedPortStatus.ToStopping(ref _status))
            {
                return;
            }

            // signal existing channels that the port is closing
            base.StopPort(timeout);

            // prevent new requests from getting processed
            StopListener();

            // wait for open channels to close
            InternalStop(timeout);

            // mark port stopped
            _status = ForwardedPortStatus.Stopped;
        }

        /// <summary>
        /// Ensures the current instance is not disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current instance is disposed.</exception>
        protected override void CheckDisposed()
        {
#if NET7_0_OR_GREATER
            ObjectDisposedException.ThrowIf(_isDisposed, this);
#else
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
#endif // NET7_0_OR_GREATER
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            base.Dispose(disposing);

            InternalDispose(disposing);
            _isDisposed = true;
        }

        private void InternalStart()
        {
            InitializePendingChannelCountdown();

            var ip = IPAddress.Any;
            if (!string.IsNullOrEmpty(BoundHost))
            {
                ip = Dns.GetHostAddresses(BoundHost)[0];
            }

            var ep = new IPEndPoint(ip, (int) BoundPort);

            _listener = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            _listener.Bind(ep);
            _listener.Listen(5);

            Session.ErrorOccured += Session_ErrorOccured;
            Session.Disconnected += Session_Disconnected;

            // consider port started when we're listening for inbound connections
            _status = ForwardedPortStatus.Started;

            StartAccept(e: null);
        }

        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e is null)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                e = new SocketAsyncEventArgs();
#pragma warning restore CA2000 // Dispose objects before losing scope
                e.Completed += AcceptCompleted;
            }
            else
            {
                // clear the socket as we're reusing the context object
                e.AcceptSocket = null;
            }

            // only accept new connections while we are started
            if (IsStarted)
            {
                try
                {
                    if (!_listener.AcceptAsync(e))
                    {
                        AcceptCompleted(sender: null, e);
                    }
                }
                catch (ObjectDisposedException)
                {
                    if (_status == ForwardedPortStatus.Stopping || _status == ForwardedPortStatus.Stopped)
                    {
                        // ignore ObjectDisposedException while stopping or stopped
                        return;
                    }

                    throw;
                }
            }
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError is SocketError.OperationAborted or SocketError.NotSocket)
            {
                // server was stopped
                return;
            }

            // capture client socket
            var clientSocket = e.AcceptSocket;

            if (e.SocketError != SocketError.Success)
            {
                // accept new connection
                StartAccept(e);

                // dispose broken client socket
                CloseClientSocket(clientSocket);
                return;
            }

            // accept new connection
            StartAccept(e);

            // process connection
            ProcessAccept(clientSocket);
        }

        private void ProcessAccept(Socket clientSocket)
        {
            // close the client socket if we're no longer accepting new connections
            if (!IsStarted)
            {
                CloseClientSocket(clientSocket);
                return;
            }

            // capture the countdown event that we're adding a count to, as we need to make sure that we'll be signaling
            // that same instance; the instance field for the countdown event is re-initialized when the port is restarted
            // and at that time there may still be pending requests
            var pendingChannelCountdown = _pendingChannelCountdown;

            pendingChannelCountdown.AddCount();

            try
            {
                using (var channel = Session.CreateChannelDirectTcpip())
                {
                    channel.Exception += Channel_Exception;

                    if (!HandleSocks(channel, clientSocket, Session.ConnectionInfo.Timeout))
                    {
                        CloseClientSocket(clientSocket);
                        return;
                    }

                    // start receiving from client socket (and sending to server)
                    channel.Bind();
                }
            }
            catch (Exception exp)
            {
                RaiseExceptionEvent(exp);
                CloseClientSocket(clientSocket);
            }
            finally
            {
                // take into account that CountdownEvent has since been disposed; when stopping the port we
                // wait for a given time for the channels to close, but once that timeout period has elapsed
                // the CountdownEvent will be disposed
                try
                {
                    _ = pendingChannelCountdown.Signal();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore any ObjectDisposedException
                }
            }
        }

        /// <summary>
        /// Initializes the <see cref="CountdownEvent"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When the port is started for the first time, a <see cref="CountdownEvent"/> is created with an initial count
        /// of <c>1</c>.
        /// </para>
        /// <para>
        /// On subsequent (re)starts, we'll dispose the current <see cref="CountdownEvent"/> and create a new one with
        /// initial count of <c>1</c>.
        /// </para>
        /// </remarks>
        private void InitializePendingChannelCountdown()
        {
            var original = Interlocked.Exchange(ref _pendingChannelCountdown, new CountdownEvent(1));
            original?.Dispose();
        }

        private bool HandleSocks(IChannelDirectTcpip channel, Socket clientSocket, TimeSpan timeout)
        {
            Closing += closeClientSocket;

            try
            {
                var version = SocketAbstraction.ReadByte(clientSocket, timeout);
                switch (version)
                {
                    case -1:
                        // SOCKS client closed connection
                        return false;
                    case 4:
                        return HandleSocks4(clientSocket, channel, timeout);
                    case 5:
                        return HandleSocks5(clientSocket, channel, timeout);
                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "SOCKS version {0} is not supported.", version));
                }
            }
            catch (SocketException ex)
            {
                // ignore exception thrown by interrupting the blocking receive as part of closing
                // the forwarded port
#if NETFRAMEWORK
                if (ex.SocketErrorCode != SocketError.Interrupted)
                {
                    RaiseExceptionEvent(ex);
                }
#else
                // Since .NET 5 the exception has been changed.
                // more info https://github.com/dotnet/runtime/issues/41585
                if (ex.SocketErrorCode != SocketError.ConnectionAborted)
                {
                    RaiseExceptionEvent(ex);
                }
#endif
                return false;
            }
            finally
            {
                // interrupt of blocking receive is now handled by channel (SOCKS4 and SOCKS5)
                // or no longer necessary
                Closing -= closeClientSocket;
            }

#pragma warning disable SA1300 // Element should begin with upper-case letter
            void closeClientSocket(object sender, EventArgs args)
            {
                CloseClientSocket(clientSocket);
            }
#pragma warning restore SA1300 // Element should begin with upper-case letter
        }

        private static void CloseClientSocket(Socket clientSocket)
        {
            if (clientSocket.Connected)
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Send);
                }
                catch (Exception)
                {
                    // ignore exception when client socket was already closed
                }
            }

            clientSocket.Dispose();
        }

        /// <summary>
        /// Interrupts the listener, and unsubscribes from <see cref="Session"/> events.
        /// </summary>
        private void StopListener()
        {
            // close listener socket
            _listener?.Dispose();

            // unsubscribe from session events
            var session = Session;
            if (session is not null)
            {
                session.ErrorOccured -= Session_ErrorOccured;
                session.Disconnected -= Session_Disconnected;
            }
        }

        /// <summary>
        /// Waits for pending channels to close.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the pending channels to close.</param>
        private void InternalStop(TimeSpan timeout)
        {
            _ = _pendingChannelCountdown.Signal();

            if (!_pendingChannelCountdown.Wait(timeout))
            {
                // TODO: log as warning
                DiagnosticAbstraction.Log("Timeout waiting for pending channels in dynamic forwarded port to close.");
            }
        }

        private void InternalDispose(bool disposing)
        {
            if (disposing)
            {
                var listener = _listener;
                if (listener is not null)
                {
                    _listener = null;
                    listener.Dispose();
                }

                var pendingRequestsCountdown = _pendingChannelCountdown;
                if (pendingRequestsCountdown is not null)
                {
                    _pendingChannelCountdown = null;
                    pendingRequestsCountdown.Dispose();
                }
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            var session = Session;
            if (session is not null)
            {
                StopPort(session.ConnectionInfo.Timeout);
            }
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            var session = Session;
            if (session is not null)
            {
                StopPort(session.ConnectionInfo.Timeout);
            }
        }

        private void Channel_Exception(object sender, ExceptionEventArgs e)
        {
            RaiseExceptionEvent(e.Exception);
        }

        private bool HandleSocks4(Socket socket, IChannelDirectTcpip channel, TimeSpan timeout)
        {
            var commandCode = SocketAbstraction.ReadByte(socket, timeout);
            if (commandCode == -1)
            {
                // SOCKS client closed connection
                return false;
            }

            var portBuffer = new byte[2];
            if (SocketAbstraction.Read(socket, portBuffer, 0, portBuffer.Length, timeout) == 0)
            {
                // SOCKS client closed connection
                return false;
            }

            var port = Pack.BigEndianToUInt16(portBuffer);

            var ipBuffer = new byte[4];
            if (SocketAbstraction.Read(socket, ipBuffer, 0, ipBuffer.Length, timeout) == 0)
            {
                // SOCKS client closed connection
                return false;
            }

            var ipAddress = new IPAddress(ipBuffer);

            var username = ReadString(socket, timeout);
            if (username is null)
            {
                // SOCKS client closed connection
                return false;
            }

            var host = ipAddress.ToString();

            RaiseRequestReceived(host, port);

            channel.Open(host, port, this, socket);

            _ = socket.Send([0x00]);

            if (channel.IsOpen)
            {
                _ = socket.Send([0x5a]);
                _ = socket.Send(portBuffer);
                _ = socket.Send(ipBuffer);
                return true;
            }

            // signal that request was rejected or failed
            _ = socket.Send([0x5b]);
            return false;
        }

        private bool HandleSocks5(Socket socket, IChannelDirectTcpip channel, TimeSpan timeout)
        {
            var authenticationMethodsCount = SocketAbstraction.ReadByte(socket, timeout);
            if (authenticationMethodsCount == -1)
            {
                // SOCKS client closed connection
                return false;
            }

            var authenticationMethods = new byte[authenticationMethodsCount];
            if (SocketAbstraction.Read(socket, authenticationMethods, 0, authenticationMethods.Length, timeout) == 0)
            {
                // SOCKS client closed connection
                return false;
            }

            if (authenticationMethods.Min() == 0)
            {
                // no user authentication is one of the authentication methods supported
                // by the SOCKS client
                _ = socket.Send([0x05, 0x00]);
            }
            else
            {
                // the SOCKS client requires authentication, which we currently do not support
                _ = socket.Send([0x05, 0xFF]);

                // we continue business as usual but expect the client to close the connection
                // so one of the subsequent reads should return -1 signaling that the client
                // has effectively closed the connection
            }

            var version = SocketAbstraction.ReadByte(socket, timeout);
            if (version == -1)
            {
                // SOCKS client closed connection
                return false;
            }

            if (version != 5)
            {
                throw new ProxyException("SOCKS5: Version 5 is expected.");
            }

            var commandCode = SocketAbstraction.ReadByte(socket, timeout);
            if (commandCode == -1)
            {
                // SOCKS client closed connection
                return false;
            }

            var reserved = SocketAbstraction.ReadByte(socket, timeout);
            if (reserved == -1)
            {
                // SOCKS client closed connection
                return false;
            }

            if (reserved != 0)
            {
                throw new ProxyException("SOCKS5: 0 is expected for reserved byte.");
            }

            var addressType = SocketAbstraction.ReadByte(socket, timeout);
            if (addressType == -1)
            {
                // SOCKS client closed connection
                return false;
            }

            var host = GetSocks5Host(addressType, socket, timeout);
            if (host is null)
            {
                // SOCKS client closed connection
                return false;
            }

            var portBuffer = new byte[2];
            if (SocketAbstraction.Read(socket, portBuffer, 0, portBuffer.Length, timeout) == 0)
            {
                // SOCKS client closed connection
                return false;
            }

            var port = Pack.BigEndianToUInt16(portBuffer);

            RaiseRequestReceived(host, port);

            channel.Open(host, port, this, socket);

            var socksReply = CreateSocks5Reply(channel.IsOpen);

            _ = socket.Send(socksReply);

            return true;
        }

        private static string GetSocks5Host(int addressType, Socket socket, TimeSpan timeout)
        {
            switch (addressType)
            {
                case 0x01: // IPv4
                    {
                        var addressBuffer = new byte[4];
                        if (SocketAbstraction.Read(socket, addressBuffer, 0, 4, timeout) == 0)
                        {
                            // SOCKS client closed connection
                            return null;
                        }

                        var ipv4 = new IPAddress(addressBuffer);
                        return ipv4.ToString();
                    }

                case 0x03: // Domain name
                    {
                        var length = SocketAbstraction.ReadByte(socket, timeout);
                        if (length == -1)
                        {
                            // SOCKS client closed connection
                            return null;
                        }

                        var addressBuffer = new byte[length];
                        if (SocketAbstraction.Read(socket, addressBuffer, 0, addressBuffer.Length, timeout) == 0)
                        {
                            // SOCKS client closed connection
                            return null;
                        }

                        var hostName = SshData.Ascii.GetString(addressBuffer, 0, addressBuffer.Length);
                        return hostName;
                    }

                case 0x04: // IPv6
                    {
                        var addressBuffer = new byte[16];
                        if (SocketAbstraction.Read(socket, addressBuffer, 0, 16, timeout) == 0)
                        {
                            // SOCKS client closed connection
                            return null;
                        }

                        var ipv6 = new IPAddress(addressBuffer);
                        return ipv6.ToString();
                    }

                default:
                    throw new ProxyException(string.Format(CultureInfo.InvariantCulture, "SOCKS5: Address type '{0}' is not supported.", addressType));
            }
        }

        private static byte[] CreateSocks5Reply(bool channelOpen)
        {
            var socksReply = new byte[// SOCKS version
                                      1 +

                                      // Reply field
                                      1 +

                                      // Reserved; fixed: 0x00
                                      1 +

                                      // Address type; fixed: 0x01
                                      1 +

                                      // IPv4 server bound address; fixed: {0x00, 0x00, 0x00, 0x00}
                                      4 +

                                      // server bound port; fixed: {0x00, 0x00}
                                      2];

            socksReply[0] = 0x05;

            if (channelOpen)
            {
                socksReply[1] = 0x00; // succeeded
            }
            else
            {
                socksReply[1] = 0x01; // general SOCKS server failure
            }

            // reserved
            socksReply[2] = 0x00;

            // IPv4 address type
            socksReply[3] = 0x01;

            return socksReply;
        }

        /// <summary>
        /// Reads a null terminated string from a socket.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to read from.</param>
        /// <param name="timeout">The timeout to apply to individual reads.</param>
        /// <returns>
        /// The <see cref="string"/> read, or <see langword="null"/> when the socket was closed.
        /// </returns>
        private static string ReadString(Socket socket, TimeSpan timeout)
        {
            var text = new StringBuilder();
            var buffer = new byte[1];

            while (true)
            {
                if (SocketAbstraction.Read(socket, buffer, 0, 1, timeout) == 0)
                {
                    // SOCKS client closed connection
                    return null;
                }

                var byteRead = buffer[0];
                if (byteRead == 0)
                {
                    // end of the string
                    break;
                }

                _ = text.Append((char) byteRead);
            }

            return text.ToString();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ForwardedPortDynamic"/> class.
        /// </summary>
        ~ForwardedPortDynamic()
        {
            Dispose(disposing: false);
        }
    }
}
