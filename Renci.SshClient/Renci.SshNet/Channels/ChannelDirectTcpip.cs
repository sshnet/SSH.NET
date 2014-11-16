using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Implements "direct-tcpip" SSH channel.
    /// </summary>
    internal partial class ChannelDirectTcpip : ClientChannel, IChannelDirectTcpip
    {
        private readonly object _socketLock = new object();

        private EventWaitHandle _channelOpen = new AutoResetEvent(false);
        private EventWaitHandle _channelData = new AutoResetEvent(false);
        private IForwardedPort _forwardedPort;
        private Socket _socket;

        /// <summary>
        /// Initializes a new <see cref="ChannelDirectTcpip"/> instance.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        public ChannelDirectTcpip(ISession session, uint localChannelNumber, uint localWindowSize, uint localPacketSize)
            : base(session, localChannelNumber, localWindowSize, localPacketSize)
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
            get { return ChannelTypes.DirectTcpip; }
        }

        public void Open(string remoteHost, uint port, IForwardedPort forwardedPort, Socket socket)
        {
            if (IsOpen)
                throw new SshException("Channel is already open.");
            if (!IsConnected)
                throw new SshException("Session is not connected.");

            _socket = socket;
            _forwardedPort = forwardedPort;
            _forwardedPort.Closing += ForwardedPort_Closing;

            var ep = socket.RemoteEndPoint as IPEndPoint;

            // open channel
            SendMessage(new ChannelOpenMessage(LocalChannelNumber, LocalWindowSize, LocalPacketSize,
                new DirectTcpipChannelInfo(remoteHost, port, ep.Address.ToString(), (uint) ep.Port)));

            //  Wait for channel to open
            WaitOnHandle(_channelOpen);
        }

        /// <summary>
        /// Occurs as the forwarded port is being stopped.
        /// </summary>
        private void ForwardedPort_Closing(object sender, EventArgs eventArgs)
        {
            // signal to the client that we will not send anything anymore; this will also interrupt the
            // blocking receive in Bind if the client sends FIN/ACK in time
            //
            // if the FIN/ACK is not sent in time, the socket will be closed in Close(bool)
            ShutdownSocket(SocketShutdown.Send);
        }

        /// <summary>
        /// Binds channel to remote host.
        /// </summary>
        public void Bind()
        {
            //  Cannot bind if channel is not open
            if (!IsOpen)
                return;

            var buffer = new byte[RemotePacketSize];

            while (_socket != null && _socket.Connected)
            {
                try
                {
                    var read = 0;
                    InternalSocketReceive(buffer, ref read);
                    if (read > 0)
                    {
                        SendMessage(new ChannelDataMessage(RemoteChannelNumber, buffer.Take(read).ToArray()));
                    }
                    else
                    {
                        // client shut down the socket (but the server may still send data or an EOF)
                        break;
                    }
                }
                catch (SocketException exp)
                {
                    switch (exp.SocketErrorCode)
                    {
                        case SocketError.WouldBlock:
                        case SocketError.IOPending:
                        case SocketError.NoBufferSpaceAvailable:
                            // socket buffer is probably empty, wait and try again
                            Thread.Sleep(30);
                            break;
                        case SocketError.ConnectionAborted:
                        case SocketError.ConnectionReset:
                            // connection was closed after receiving SSH_MSG_CHANNEL_CLOSE message
                            break;
                        case SocketError.Interrupted:
                            // connection was closed because FIN/ACK was not received in time after
                            // shutting down the (send part of the) socket
                            break;
                        default:
                            throw; // throw any other error
                    }
                }
            }

            // even though the client has disconnected, we still want to properly close the
            // channel
            //
            // we'll do this in in Close(bool) that way we have a single place from which we
            // send an SSH_MSG_CHANNEL_EOF message and wait for the SSH_MSG_CHANNEL_CLOSE
            // message
        }

        /// <summary>
        /// Closes the socket, hereby interrupting the blocking receive in <see cref="Bind()"/>.
        /// </summary>
        private void CloseSocket()
        {
            if (_socket == null)
                return;

            lock (_socketLock)
            {
                if (_socket == null)
                    return;

                // closing a socket actually disposes the socket, so we can safely dereference
                // the field to avoid entering the lock again later
                _socket.Close();
                _socket = null;
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

            lock (_socketLock)
            {
                if (_socket == null || !_socket.Connected)
                    return;

                _socket.Shutdown(how);
            }
        }

        /// <summary>
        /// Closes the channel, optionally waiting for the SSH_MSG_CHANNEL_CLOSE message to
        /// be received from the server.
        /// </summary>
        /// <param name="wait"><c>true</c> to wait for the SSH_MSG_CHANNEL_CLOSE message to be received from the server; otherwise, <c>false</c>.</param>
        protected override void Close(bool wait)
        {
            if (_forwardedPort != null)
            {
                _forwardedPort.Closing -= ForwardedPort_Closing;
                _forwardedPort = null;
            }

            // signal to the client that we will not send anything anymore; this will also interrupt the
            // blocking receive in Bind if the client sends FIN/ACK in time
            //
            // if the FIN/ACK is not sent in time, the socket will be closed after the channel is closed
            ShutdownSocket(SocketShutdown.Send);

            // close the SSH channel, and mark the channel closed
            base.Close(wait);

            // close the socket
            CloseSocket();
        }

        /// <summary>
        /// Called when channel data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected override void OnData(byte[] data)
        {
            base.OnData(data);

            if (_socket != null && _socket.Connected)
            {
                lock (_socketLock)
                {
                    if (_socket != null && _socket.Connected)
                    {
                        InternalSocketSend(data);
                    }
                }
            }
        }

        /// <summary>
        /// Called when channel is opened by the server.
        /// </summary>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="initialWindowSize">Initial size of the window.</param>
        /// <param name="maximumPacketSize">Maximum size of the packet.</param>
        protected override void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            base.OnOpenConfirmation(remoteChannelNumber, initialWindowSize, maximumPacketSize);

            _channelOpen.Set();
        }

        protected override void OnOpenFailure(uint reasonCode, string description, string language)
        {
            base.OnOpenFailure(reasonCode, description, language);

            _channelOpen.Set();
        }

        /// <summary>
        /// Called when channel has no more data to receive.
        /// </summary>
        protected override void OnEof()
        {
            base.OnEof();

            // the channel will send no more data, and hence it does not make sense to receive
            // any more data from the client to send to the remote party (and we surely won't
            // send anything anymore)
            //
            // this will also interrupt the blocking receive in Bind()
            ShutdownSocket(SocketShutdown.Send);
        }

        /// <summary>
        /// Called whenever an unhandled <see cref="Exception"/> occurs in <see cref="Session"/> causing
        /// the message loop to be interrupted, or when an exception occurred processing a channel message.
        /// </summary>
        protected override void OnErrorOccured(Exception exp)
        {
            base.OnErrorOccured(exp);

            // signal to the client that we will not send anything anymore; this will also interrupt the
            // blocking receive in Bind if the client sends FIN/ACK in time
            //
            // if the FIN/ACK is not sent in time, the socket will be closed in Close(bool)
            ShutdownSocket(SocketShutdown.Send);
        }

        /// <summary>
        /// Called when the server wants to terminate the connection immmediately.
        /// </summary>
        /// <remarks>
        /// The sender MUST NOT send or receive any data after this message, and
        /// the recipient MUST NOT accept any data after receiving this message.
        /// </remarks>
        protected override void OnDisconnected()
        {
            base.OnDisconnected();

            // the channel will accept or send no more data, and hence it does not make sense
            // to accept any more data from the client (and we surely won't send anything
            // anymore)
            //
            // 
            // so lets signal to the client that we will not send or receive anything anymore
            // this will also interrupt the blocking receive in Bind()
            ShutdownSocket(SocketShutdown.Both);
        }

        partial void InternalSocketReceive(byte[] buffer, ref int read);

        partial void InternalSocketSend(byte[] data);

        protected override void Dispose(bool disposing)
        {
            // make sure we've unsubscribed from all session events and closed the channel
            // before we starting disposing
            base.Dispose(disposing);

            if (disposing)
            {
                if (_socket != null)
                {
                    lock (_socketLock)
                    {
                        if (_socket != null)
                        {
                            _socket.Dispose();
                            _socket = null;
                        }
                    }
                }

                if (_channelOpen != null)
                {
                    _channelOpen.Dispose();
                    _channelOpen = null;
                }

                if (_channelData != null)
                {
                    _channelData.Dispose();
                    _channelData = null;
                }
            }
        }
    }
}
