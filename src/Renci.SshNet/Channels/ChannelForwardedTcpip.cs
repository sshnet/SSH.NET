using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Implements "forwarded-tcpip" SSH channel.
    /// </summary>
    internal class ChannelForwardedTcpip : ServerChannel, IChannelForwardedTcpip
    {
        private readonly object _socketShutdownAndCloseLock = new object();
        private Socket _socket;
        private IForwardedPort _forwardedPort;

        private bool doSocks;
        private bool doSocks5;
        private ManualResetEvent completionWaitHandle;

        /// <summary>
        /// Initializes a new <see cref="ChannelForwardedTcpip"/> instance.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="remoteWindowSize">The window size of the remote party.</param>
        /// <param name="remotePacketSize">The maximum size of a data packet that we can send to the remote party.</param>
        internal ChannelForwardedTcpip(ISession session,
                                       uint localChannelNumber,
                                       uint localWindowSize,
                                       uint localPacketSize,
                                       uint remoteChannelNumber,
                                       uint remoteWindowSize,
                                       uint remotePacketSize)
            : base(session,
                   localChannelNumber,
                   localWindowSize,
                   localPacketSize,
                   remoteChannelNumber,
                   remoteWindowSize,
                   remotePacketSize)
        {
        }

        /// <summary>
        /// Gets the type of the channel.
        /// </summary>
        /// <value>
        /// The type of the channel.
        /// </value>
        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.ForwardedTcpip; }
        }

        /// <summary>
        /// Binds the channel to the specified endpoint.
        /// </summary>
        /// <param name="remoteEndpoint">The endpoint to connect to.</param>
        /// <param name="forwardedPort">The forwarded port for which the channel is opened.</param>
        public void Bind(IPEndPoint remoteEndpoint, IForwardedPort forwardedPort)
        {
            if (!IsConnected)
            {
                throw new SshException("Session is not connected.");
            }

            _forwardedPort = forwardedPort;
            _forwardedPort.Closing += ForwardedPort_Closing;

            if (remoteEndpoint == null)
            {
                doSocks = true;
                SendMessage(new ChannelOpenConfirmationMessage(RemoteChannelNumber, LocalWindowSize, LocalPacketSize, LocalChannelNumber));

                completionWaitHandle = new ManualResetEvent(false);
                completionWaitHandle.WaitOne();
                completionWaitHandle.Dispose();
            }

            // Try to connect to the socket
            try
            {
                _socket = SocketAbstraction.Connect(remoteEndpoint, ConnectionInfo.Timeout);

                // send channel open confirmation message
                SendMessage(new ChannelOpenConfirmationMessage(RemoteChannelNumber, LocalWindowSize, LocalPacketSize, LocalChannelNumber));
            }
            catch (Exception exp)
            {
                // send channel open failure message
                SendMessage(new ChannelOpenFailureMessage(RemoteChannelNumber, exp.ToString(), ChannelOpenFailureMessage.ConnectFailed, "en"));

                throw;
            }

            var buffer = new byte[RemotePacketSize];

            SocketAbstraction.ReadContinuous(_socket, buffer, 0, buffer.Length, SendData);
        }

        protected override void OnErrorOccured(Exception exp)
        {
            base.OnErrorOccured(exp);

            // signal to the server that we will not send anything anymore; this will also interrupt the
            // blocking receive in Bind if the server sends FIN/ACK in time
            //
            // if the FIN/ACK is not sent in time, the socket will be closed in Close(bool)
            ShutdownSocket(SocketShutdown.Send);
        }

        /// <summary>
        /// Occurs as the forwarded port is being stopped.
        /// </summary>
        private void ForwardedPort_Closing(object sender, EventArgs eventArgs)
        {
            // signal to the server that we will not send anything anymore; this will also interrupt the
            // blocking receive in Bind if the server sends FIN/ACK in time
            //
            // if the FIN/ACK is not sent in time, the socket will be closed in Close(bool)
            ShutdownSocket(SocketShutdown.Send);

            if (completionWaitHandle != null)
            {
                completionWaitHandle.Set();
            }
        }

        /// <summary>
        /// Shuts down the socket.
        /// </summary>
        /// <param name="how">One of the <see cref="SocketShutdown"/> values that specifies the operation that will no longer be allowed.</param>
        private void ShutdownSocket(SocketShutdown how)
        {
            if (_socket == null)
                return;

            lock (_socketShutdownAndCloseLock)
            {
                var socket = _socket;
                if (!socket.IsConnected())
                    return;

                try
                {
                    socket.Shutdown(how);
                }
                catch (SocketException ex)
                {
                    // TODO: log as warning
                    DiagnosticAbstraction.Log("Failure shutting down socket: " + ex);
                }
            }
        }

        /// <summary>
        /// Closes the socket, hereby interrupting the blocking receive in <see cref="Bind(IPEndPoint,IForwardedPort)"/>.
        /// </summary>
        private void CloseSocket()
        {
            if (_socket == null)
                return;

            lock (_socketShutdownAndCloseLock)
            {
                var socket = _socket;
                if (socket != null)
                {
                    _socket = null;
                    socket.Dispose();
                }
            }
        }

        /// <summary>
        /// Closes the channel waiting for the SSH_MSG_CHANNEL_CLOSE message to be received from the server.
        /// </summary>
        protected override void Close()
        {
            var forwardedPort = _forwardedPort;
            if (forwardedPort != null)
            {
                forwardedPort.Closing -= ForwardedPort_Closing;
                _forwardedPort = null;
            }

            // signal to the server that we will not send anything anymore; this will also interrupt the
            // blocking receive in Bind if the server sends FIN/ACK in time
            //
            // if the FIN/ACK is not sent in time, the socket will be closed after the channel is closed
            ShutdownSocket(SocketShutdown.Send);

            // close the SSH channel, and mark the channel closed
            base.Close();

            // close the socket
            CloseSocket();
        }

        /// <summary>
        /// Called when channel data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected override void OnData(byte[] data)
        {
            if (doSocks)
            {
                var stream = new MemoryStream(data);
                HandleSocks(stream);
                return;
            }

            base.OnData(data);

            var socket = _socket;
            if (socket.IsConnected())
            {
                SocketAbstraction.Send(socket, data, 0, data.Length);
            }
        }

        private void HandleSocks(MemoryStream stream)
        {
            var version = ReadByte(stream);
            switch (version)
            {
                case 4:
                    HandleSocks4(stream);
                    doSocks = false;
                    return;
                case 5:
                    if (!doSocks5)
                    {
                        var authenticationMethodsCount = ReadByte(stream);
                        var authenticationMethods = new byte[authenticationMethodsCount];
                        if (stream.Read(authenticationMethods, 0, authenticationMethods.Length) == 0)
                        {
                            return;
                        }

                        if (authenticationMethods.Min() == 0)
                        {
                            // no user authentication is one of the authentication methods supported
                            // by the SOCKS client
                            SendData(new byte[] { 0x05, 0x00 });
                        }
                        else
                        {
                            // the SOCKS client requires authentication, which we currently do not support
                            SendData(new byte[] { 0x05, 0xFF });
                        }
                        doSocks5 = true;
                        return;
                    }
                    HandleSocks5(stream);
                    doSocks = false;
                    return;
            }
            throw new NotSupportedException(string.Format("SOCKS version {0} is not supported.", version));
        }

        private void HandleSocks4(MemoryStream stream)
        {
            var commandCode = ReadByte(stream);
            if (commandCode == -1)
            {
                return;
            }

            var portBuffer = new byte[2];
            if (stream.Read(portBuffer, 0, portBuffer.Length) == 0)
            {
                return;
            }

            var port = (portBuffer[0] * 256 + portBuffer[1]);

            var ipBuffer = new byte[4];
            if (stream.Read(ipBuffer, 0, ipBuffer.Length) == 0)
            {
                return;
            }

            var ipAddress = new IPAddress(ipBuffer);

            ThreadAbstraction.ExecuteThread(() =>
            {
                var endpoint = new IPEndPoint(ipAddress, port);

                try
                {
                    _socket = SocketAbstraction.Connect(endpoint, ConnectionInfo.Timeout);
                }
                catch (Exception exp)
                {
                    // send channel open failure message
                    SendMessage(new ChannelOpenFailureMessage(RemoteChannelNumber, exp.ToString(), ChannelOpenFailureMessage.ConnectFailed, "en"));
                    completionWaitHandle.Set();
                    throw;
                }

                SendData(new byte[] { 0x00, 0x5a });
                SendData(portBuffer);
                SendData(ipBuffer);

                var buffer = new byte[RemotePacketSize];
                SocketAbstraction.ReadContinuous(_socket, buffer, 0, buffer.Length, SendData);
            });
        }

        private void HandleSocks5(MemoryStream stream)
        {
            var commandCode = ReadByte(stream);
            if (commandCode == -1)
            {
                return;
            }

            var reserved = ReadByte(stream);
            if (reserved == -1)
            {
                return;
            }

            if (reserved != 0)
            {
                throw new ProxyException("SOCKS5: 0 is expected for reserved byte.");
            }

            var addressType = ReadByte(stream);
            if (addressType == -1)
            {
                // SOCKS client closed connection
                return;
            }

            var ipAddress = GetSocks5Host(addressType, stream);
            if (ipAddress == null)
            {
                // SOCKS client closed connection
                return;
            }

            var portBuffer = new byte[2];
            if (stream.Read(portBuffer, 0, portBuffer.Length) == 0)
            {
                return;
            }

            var port = (portBuffer[0] * 256 + portBuffer[1]);

            ThreadAbstraction.ExecuteThread(() =>
            {
                var endpoint = new IPEndPoint(ipAddress, port);

                try
                {
                    _socket = SocketAbstraction.Connect(endpoint, ConnectionInfo.Timeout);
                }
                catch
                {
                    // send channel open failure message
                    SendData(CreateSocks5Reply(false));
                    completionWaitHandle.Set();
                    throw;
                }

                SendData(CreateSocks5Reply(true));

                var buffer = new byte[RemotePacketSize];
                SocketAbstraction.ReadContinuous(_socket, buffer, 0, buffer.Length, SendData);
            });
        }

        private IPAddress GetSocks5Host(int addressType, MemoryStream stream)
        {
            switch (addressType)
            {
                case 0x01: // IPv4
                    {
                        var addressBuffer = new byte[4];
                        if (stream.Read(addressBuffer, 0, 4) == 0)
                        {
                            // SOCKS client closed connection
                            return null;
                        }

                        return new IPAddress(addressBuffer);
                    }
                case 0x03: // Domain name
                    {
                        var length = ReadByte(stream);
                        if (length == -1)
                        {
                            // SOCKS client closed connection
                            return null;
                        }
                        var addressBuffer = new byte[length];
                        if (stream.Read(addressBuffer, 0, addressBuffer.Length) == 0)
                        {
                            // SOCKS client closed connection
                            return null;
                        }

                        var hostName = SshData.Ascii.GetString(addressBuffer, 0, addressBuffer.Length);
                        return DnsAbstraction.GetHostAddresses(hostName)[0];
                    }
                case 0x04: // IPv6
                    {
                        var addressBuffer = new byte[16];
                        if (stream.Read(addressBuffer, 0, 16) == 0)
                        {
                            return null;
                        }

                        return new IPAddress(addressBuffer);
                    }
                default:
                    throw new ProxyException(string.Format("SOCKS5: Address type '{0}' is not supported.", addressType));
            }
        }

        private static byte[] CreateSocks5Reply(bool success)
        {
            var socksReply = new byte[
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
                2];

            socksReply[0] = 0x05;

            if (success)
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

        private int ReadByte(MemoryStream stream)
        {
            var buffer = new byte[1];
            if (stream.Read(buffer, 0, 1) == 0)
                return -1;

            return buffer[0];
        }
    }
}