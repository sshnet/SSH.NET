using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    public partial class ForwardedPortDynamic
    {
        private Socket _listener;
        private CountdownEvent _pendingChannelCountdown;

        partial void InternalStart()
        {
            InitializePendingChannelCountdown();

            var ip = IPAddress.Any;
            if (!string.IsNullOrEmpty(BoundHost))
            {
                ip = DnsAbstraction.GetHostAddresses(BoundHost)[0];
            }

            var ep = new IPEndPoint(ip, (int) BoundPort);

            _listener = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {NoDelay = true};
            _listener.Bind(ep);
            _listener.Listen(5);

            Session.ErrorOccured += Session_ErrorOccured;
            Session.Disconnected += Session_Disconnected;

            // consider port started when we're listening for inbound connections
            _status = ForwardedPortStatus.Started;

            StartAccept(null);
        }

        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
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
                        AcceptCompleted(null, e);
                    }
                }
                catch (ObjectDisposedException)
                {
                    if (_status == ForwardedPortStatus.Stopped || _status == ForwardedPortStatus.Stopped)
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
            if (e.SocketError == SocketError.OperationAborted || e.SocketError == SocketError.NotSocket)
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
                    pendingChannelCountdown.Signal();
                }
                catch (ObjectDisposedException)
                {
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
            if (original != null)
            {
                original.Dispose();
            }
        }

        private bool HandleSocks(IChannelDirectTcpip channel, Socket clientSocket, TimeSpan timeout)
        {
            // create eventhandler which is to be invoked to interrupt a blocking receive
            // when we're closing the forwarded port
            EventHandler closeClientSocket = (_, args) => CloseClientSocket(clientSocket);

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
                        throw new NotSupportedException(string.Format("SOCKS version {0} is not supported.", version));
                }
            }
            catch (SocketException ex)
            {
                // ignore exception thrown by interrupting the blocking receive as part of closing
                // the forwarded port
                if (ex.SocketErrorCode != SocketError.Interrupted)
                {
                    RaiseExceptionEvent(ex);
                }
                return false;
            }
            finally
            {
                // interrupt of blocking receive is now handled by channel (SOCKS4 and SOCKS5)
                // or no longer necessary
                Closing -= closeClientSocket;
            }

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
        partial void StopListener()
        {
            // close listener socket
            var listener = _listener;
            if (listener != null)
            {
                listener.Dispose();
            }

            // unsubscribe from session events
            var session = Session;
            if (session != null)
            {
                session.ErrorOccured -= Session_ErrorOccured;
                session.Disconnected -= Session_Disconnected;
            }
        }

        /// <summary>
        /// Waits for pending channels to close.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the pending channels to close.</param>
        partial void InternalStop(TimeSpan timeout)
        {
            _pendingChannelCountdown.Signal();
            if (!_pendingChannelCountdown.Wait(timeout))
            {
                // TODO: log as warning
                DiagnosticAbstraction.Log("Timeout waiting for pending channels in dynamic forwarded port to close.");
            }

        }

        partial void InternalDispose(bool disposing)
        {
            if (disposing)
            {
                var listener = _listener;
                if (listener != null)
                {
                    _listener = null;
                    listener.Dispose();
                }

                var pendingRequestsCountdown = _pendingChannelCountdown;
                if (pendingRequestsCountdown != null)
                {
                    _pendingChannelCountdown = null;
                    pendingRequestsCountdown.Dispose();
                }
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            var session = Session;
            if (session != null)
            {
                StopPort(session.ConnectionInfo.Timeout);
            }
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            var session = Session;
            if (session != null)
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

            //  TODO:   See what need to be done depends on the code

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
            if (username == null)
            {
                // SOCKS client closed connection
                return false;
            }

            var host = ipAddress.ToString();

            RaiseRequestReceived(host, port);

            channel.Open(host, port, this, socket);

            SocketAbstraction.SendByte(socket, 0x00);

            if (channel.IsOpen)
            {
                SocketAbstraction.SendByte(socket, 0x5a);
                SocketAbstraction.Send(socket, portBuffer, 0, portBuffer.Length);
                SocketAbstraction.Send(socket, ipBuffer, 0, ipBuffer.Length);
                return true;
            }

            // signal that request was rejected or failed
            SocketAbstraction.SendByte(socket, 0x5b);
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
                SocketAbstraction.Send(socket, new byte[] {0x05, 0x00}, 0, 2);
            }
            else
            {
                // the SOCKS client requires authentication, which we currently do not support
                SocketAbstraction.Send(socket, new byte[] {0x05, 0xFF}, 0, 2);

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
                throw new ProxyException("SOCKS5: Version 5 is expected.");

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
            if (host == null)
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

            SocketAbstraction.Send(socket, socksReply, 0, socksReply.Length);

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
                    throw new ProxyException(string.Format("SOCKS5: Address type '{0}' is not supported.", addressType));
            }
        }

        private static byte[] CreateSocks5Reply(bool channelOpen)
        {
            var socksReply = new byte
                [
                    // SOCKS version
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
                    2
                ];

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
        /// The <see cref="string"/> read, or <c>null</c> when the socket was closed.
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

                var c = (char) byteRead;
                text.Append(c);
            }
            return text.ToString();
        }
    }
}

