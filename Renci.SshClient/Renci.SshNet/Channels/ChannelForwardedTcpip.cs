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

        /// <summary>
        /// Holds a value indicating whether the SSH_MSG_CHANNEL_EOF has been sent to the client.
        /// </summary>
        /// <value>
        /// <c>0</c> when the SSH_MSG_CHANNEL_EOF message has not been sent to the client, and
        /// <c>1</c> when this message was already sent.
        /// </value>
        private int _sentEof;

        private IForwardedPort _forwardedPort;

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
                SendMessage(new ChannelOpenFailureMessage(RemoteChannelNumber, exp.ToString(), 2));

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

            // close the socket, hereby interrupting the blocking receive in Bind(IPEndPoint,IForwardedPort)
            CloseSocket();
        }

        /// <summary>
        /// Occurs as the forwarded port is being stopped.
        /// </summary>
        private void ForwardedPort_Closing(object sender, EventArgs eventArgs)
        {
            // close the socket, hereby interrupting the blocking receive in Bind(IPEndPoint,IForwardedPort)
            CloseSocket();
        }

        partial void OpenSocket(IPEndPoint remoteEndpoint);

        /// <summary>
        /// Closes the socket, hereby interrupting the blocking receive in <see cref="Bind(IPEndPoint,IForwardedPort)"/>.
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

            //  send EOF message first when channel need to be closed
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

        partial void InternalSocketSend(byte[] data);
        
        partial void InternalSocketReceive(byte[] buffer, ref int read);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
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

            base.Dispose(disposing);
        }
    }
}
