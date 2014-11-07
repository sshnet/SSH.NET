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
        private readonly object _socketShutdownAndCloseLock = new object();

        private EventWaitHandle _channelEof = new AutoResetEvent(false);
        private EventWaitHandle _channelOpen = new AutoResetEvent(false);
        private EventWaitHandle _channelData = new AutoResetEvent(false);

        /// <summary>
        /// An <see cref="EventWaitHandle"/> that is signaled when the blocking receive is cancelled because the
        /// forwarded port is closing.
        /// </summary>
        private EventWaitHandle _channelInterrupted = new ManualResetEvent(false);

        private IForwardedPort _forwardedPort;
        private Socket _socket;

        /// <summary>
        /// Holds a value indicating whether the SSH_MSG_CHANNEL_EOF has been sent to the server.
        /// </summary>
        /// <value>
        /// <c>0</c> when the SSH_MSG_CHANNEL_EOF message has not been sent to the server, and
        /// <c>1</c> when this message was already sent.
        /// </value>
        private int _sentEof;

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
            _sentEof = 0;

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
            CloseSocket();
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
                        // client quit sending (but the server may still send data or an EOF)
                        if (Interlocked.CompareExchange(ref _sentEof, 1, 0) == 0)
                        {
                            // inform server that we won't be sending anything anymore if we
                            // haven't already sent a SSH_MSG_CHANNEL_EOF message
                            //
                            // note that we'll still wait for a SSH_MSG_CHANNEL_EOF or
                            // SSH_MSG_CHANNEL_CLOSE message once we've broken the receive
                            // loop
                            SendEof();
                        }
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
                            // in which case the _channelEof waithandle is also set
                            break;
                        case SocketError.Interrupted:
                            // connection was interrupted as part of closing the forwarded port
                            _channelInterrupted.Set();
                            break;
                        default:
                            throw; // throw any other error
                    }
                }
            }

            WaitHandle.WaitAny(new WaitHandle[] {_channelEof, _channelInterrupted});
        }

        /// <summary>
        /// Closes the socket, hereby interrupting the blocking receive in <see cref="Bind()"/>.
        /// </summary>
        private void CloseSocket()
        {
            if (_socket == null || !_socket.Connected)
                return;

            lock (_socketShutdownAndCloseLock)
            {
                if (_socket == null || !_socket.Connected)
                    return;

                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }

        private void ShutdownSocket(SocketShutdown how)
        {
            if (_socket == null || !_socket.Connected)
                return;

            lock (_socketShutdownAndCloseLock)
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

            // close the socket, hereby interrupting the blocking receive in Bind()
            CloseSocket();

            //  send EOF message first when channel needs to be closed
            if (IsOpen && Interlocked.CompareExchange(ref _sentEof, 1, 0) == 0)
            {
                SendEof();
            }

            base.Close(wait);
        }

        /// <summary>
        /// Called when channel data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected override void OnData(byte[] data)
        {
            base.OnData(data);

            if (_socket != null && _socket.Connected)
                InternalSocketSend(data);
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

            // the channel will send no more data, so signal to the client that
            // we won't be sending anything anymore
            ShutdownSocket(SocketShutdown.Send);

            var channelEof = _channelEof;
            if (channelEof != null)
                channelEof.Set();
        }

        protected override void OnClose()
        {
            base.OnClose();

            // the channel will send no more data, so signal to the client that
            // we won't be sending anything anymore
            //
            // we need to do this here in case the server sends the SSH_MSG_CHANNEL_CLOSE
            // message without first sending SSH_MSG_CHANNEL_EOF
            ShutdownSocket(SocketShutdown.Send);

            var channelEof = _channelEof;
            if (channelEof != null)
                channelEof.Set();
        }

        /// <summary>
        /// Called whenever an unhandled <see cref="Exception"/> occurs in <see cref="Session"/> causing
        /// the message loop to be interrupted, or when an exception occurred processing a channel message.
        /// </summary>
        protected override void OnErrorOccured(Exception exp)
        {
            base.OnErrorOccured(exp);

            // close the socket, hereby interrupting the blocking receive in Bind()
            CloseSocket();

            //  if error occured, no more data can be received
            var channelEof = _channelEof;
            if (channelEof != null)
                channelEof.Set();
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

            // close the socket, hereby interrupting the blocking receive in Bind()
            CloseSocket();

            //  If disconnected, no more data can be received
            var channelEof = _channelEof;
            if (channelEof != null)
                channelEof.Set();
        }

        partial void InternalSocketReceive(byte[] buffer, ref int read);

        partial void InternalSocketSend(byte[] data);

        protected override void Dispose(bool disposing)
        {
            // make sure we've unsubscribed from all session events before we starting disposing
            base.Dispose(disposing);

            if (_forwardedPort != null)
            {
                _forwardedPort.Closing -= ForwardedPort_Closing;
                _forwardedPort = null;
            }

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            if (_channelEof != null)
            {
                _channelEof.Dispose();
                _channelEof = null;
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

            if (_channelInterrupted != null)
            {
                _channelInterrupted.Dispose();
                _channelInterrupted = null;
            }
        }
    }
}
