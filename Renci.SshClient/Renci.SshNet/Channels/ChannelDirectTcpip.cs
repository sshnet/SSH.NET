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
        private EventWaitHandle _channelEof = new AutoResetEvent(false);
        private EventWaitHandle _channelOpen = new AutoResetEvent(false);
        private EventWaitHandle _channelData = new AutoResetEvent(false);
        private EventWaitHandle _channelInterrupted = new ManualResetEvent(false);
        private IForwardedPort _forwardedPort;
        private Socket _socket;

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
            if (!this.IsConnected)
                throw new SshException("Session is not connected.");

            _socket = socket;
            _forwardedPort = forwardedPort;
            _forwardedPort.Closing += ForwardedPort_Closing;

            var ep = socket.RemoteEndPoint as IPEndPoint;

            //  Open channel
            this.SendMessage(new ChannelOpenMessage(this.LocalChannelNumber, this.LocalWindowSize, this.LocalPacketSize,
                                                        new DirectTcpipChannelInfo(remoteHost, port, ep.Address.ToString(), (uint)ep.Port)));

            //  Wait for channel to open
            this.WaitOnHandle(this._channelOpen);
        }

        private void ForwardedPort_Closing(object sender, EventArgs eventArgs)
        {
            // close the socket, hereby interrupting the blocking receive in Bind()
            if (_socket != null)
                CloseSocket();
        }

        /// <summary>
        /// Binds channel to remote host.
        /// </summary>
        public void Bind()
        {
            //  Cannot bind if channel is not open
            if (!this.IsOpen)
                return;

            var buffer = new byte[this.RemotePacketSize];

            while (this._socket != null && _socket.Connected)
            {
                try
                {
                    var read = 0;
                    this.InternalSocketReceive(buffer, ref read);
                    if (read > 0)
                    {
                        this.SendMessage(new ChannelDataMessage(this.RemoteChannelNumber, buffer.Take(read).ToArray()));
                    }
                    else
                    {
                        // client quit sending
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

            WaitHandle.WaitAny(new WaitHandle[] { _channelEof, _channelInterrupted });
        }

        /// <summary>
        /// Closes the socket, hereby interrupting the blocking receive in <see cref="Bind()"/>.
        /// </summary>
        private void CloseSocket()
        {
            if (!_socket.Connected)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        /// <summary>
        /// Closes the channel.
        /// </summary>
        public override void Close()
        {
            if (_forwardedPort != null)
            {
                _forwardedPort.Closing -= ForwardedPort_Closing;
                _forwardedPort = null;
            }

            // close the socket, hereby interrupting the blocking receive in Bind()
            if (this._socket != null)
                CloseSocket();

            //  Send EOF message first when channel need to be closed
            this.SendMessage(new ChannelEofMessage(this.RemoteChannelNumber));

            base.Close();
        }

        /// <summary>
        /// Called when channel data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected override void OnData(byte[] data)
        {
            base.OnData(data);

            this.InternalSocketSend(data);
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

            this._channelOpen.Set();
        }

        protected override void OnOpenFailure(uint reasonCode, string description, string language)
        {
            base.OnOpenFailure(reasonCode, description, language);

            this._channelOpen.Set();
        }

        /// <summary>
        /// Called when channel has no more data to receive.
        /// </summary>
        protected override void OnEof()
        {
            base.OnEof();

            // the channel will send no more data, so signal to the client that
            // we won't be sending anything anymore
            if (_socket != null && _socket.Connected)
                _socket.Shutdown(SocketShutdown.Send);

            var channelEof = this._channelEof;
            if (channelEof != null)
                channelEof.Set();
        }

        protected override void OnClose()
        {
            base.OnClose();

            var channelEof = this._channelEof;
            if (channelEof != null)
                channelEof.Set();
        }

        /// <summary>
        /// Called whenever an unhandled <see cref="Exception"/> occurs in <see cref="Session"/> causing
        /// the message loop to be interrupted.
        /// </summary>
        protected override void OnErrorOccured(Exception exp)
        {
            base.OnErrorOccured(exp);

            // close the socket, hereby interrupting the blocking receive in Bind()
            if (_socket != null)
                CloseSocket();

            //  if error occured, no more data can be received
            var channelEof = this._channelEof;
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
            if (_socket != null)
                CloseSocket();

            //  If disconnected, no more data can be received
            var channelEof = this._channelEof;
            if (channelEof != null)
                channelEof.Set();
        }

        partial void InternalSocketReceive(byte[] buffer, ref int read);

        partial void InternalSocketSend(byte[] data);

        protected override void Dispose(bool disposing)
        {
            if (_forwardedPort != null)
            {
                _forwardedPort.Closing -= ForwardedPort_Closing;
                _forwardedPort = null;
            }

            if (this._socket != null)
            {
                this._socket.Dispose();
                this._socket = null;
            }

            if (this._channelEof != null)
            {
                this._channelEof.Dispose();
                this._channelEof = null;
            }

            if (this._channelOpen != null)
            {
                this._channelOpen.Dispose();
                this._channelOpen = null;
            }

            if (this._channelData != null)
            {
                this._channelData.Dispose();
                this._channelData = null;
            }

            if (_channelInterrupted != null)
            {
                _channelInterrupted.Dispose();
                _channelInterrupted = null;
            }

            base.Dispose(disposing);
        }
    }
}
