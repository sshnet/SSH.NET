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
    /// Implements "forwarded-tcpip" SSH channel.
    /// </summary>
    internal partial class ChannelForwardedTcpip : ServerChannel, IChannelForwardedTcpip
    {
        private readonly object _socketShutdownAndCloseLock = new object();
        private Socket _socket;
        private IForwardedPort _forwardedPort;

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
        internal ChannelForwardedTcpip(ISession session, uint localChannelNumber, uint localWindowSize, uint localPacketSize, uint remoteChannelNumber, uint remoteWindowSize, uint remotePacketSize)
            : base(session, localChannelNumber, localWindowSize, localPacketSize, remoteChannelNumber, remoteWindowSize, remotePacketSize)
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
            byte[] buffer;

            if (!IsConnected)
            {
                throw new SshException("Session is not connected.");
            }

            _forwardedPort = forwardedPort;
            _forwardedPort.Closing += ForwardedPort_Closing;

            //  Try to connect to the socket 
            try
            {
                //  Get buffer in memory for data exchange
                buffer = new byte[RemotePacketSize];

                OpenSocket(remoteEndpoint);

                // send channel open confirmation message
                SendMessage(new ChannelOpenConfirmationMessage(RemoteChannelNumber, LocalWindowSize, LocalPacketSize, LocalChannelNumber));
            }
            catch (Exception exp)
            {
                // send channel open failure message
                SendMessage(new ChannelOpenFailureMessage(RemoteChannelNumber, exp.ToString(), ChannelOpenFailureMessage.ConnectFailed, "en"));

                throw;
            }

            //  Start reading data from the port and send to channel
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
                        // server quit sending
                        break;
                    }
                }
                catch (SocketException exp)
                {
                    if (exp.SocketErrorCode == SocketError.WouldBlock ||
                        exp.SocketErrorCode == SocketError.IOPending ||
                        exp.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);
                    }
                    else if (exp.SocketErrorCode == SocketError.ConnectionAborted || exp.SocketErrorCode == SocketError.Interrupted)
                    {
                        break;
                    }
                    else
                        throw;  // throw any other error
                }
            }
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
        }

        partial void OpenSocket(IPEndPoint remoteEndpoint);

        /// <summary>
        /// Shuts down the socket.
        /// </summary>
        /// <param name="how">One of the <see cref="SocketShutdown"/> values that specifies the operation that will no longer be allowed.</param>
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
        /// Closes the socket, hereby interrupting the blocking receive in <see cref="Bind(IPEndPoint,IForwardedPort)"/>.
        /// </summary>
        private void CloseSocket()
        {
            if (_socket == null)
                return;

            lock (_socketShutdownAndCloseLock)
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

            // signal to the server that we will not send anything anymore; this will also interrupt the
            // blocking receive in Bind if the server sends FIN/ACK in time
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
                InternalSocketSend(data);
        }

        partial void InternalSocketSend(byte[] data);
        
        partial void InternalSocketReceive(byte[] buffer, ref int read);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // make sure we've unsubscribed from all session events and closed the channel
            // before we starting disposing
            base.Dispose(disposing);

            if (disposing)
            {
                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }
    }
}
