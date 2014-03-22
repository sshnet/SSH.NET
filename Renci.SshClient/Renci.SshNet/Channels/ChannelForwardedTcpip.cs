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
    internal partial class ChannelForwardedTcpip : ServerChannel
    {
        private Socket _socket;

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
        /// Initializes a new instance of the <see cref="ChannelForwardedTcpip"/> class.
        /// </summary>
        public ChannelForwardedTcpip()
        {
        }

        /// <summary>
        /// Binds channel to specified connected host.
        /// </summary>
        /// <param name="connectedHost">The connected host.</param>
        /// <param name="connectedPort">The connected port.</param>
        public void Bind(IPAddress connectedHost, uint connectedPort)
        {
            byte[] buffer;

            if (!this.IsConnected)
            {
                throw new SshException("Session is not connected.");
            }

            //  Try to connect to the socket 
            try
            {
                //  Get buffer in memory for data exchange
                buffer = new byte[this.RemotePacketSize];

                this.OpenSocket(connectedHost, connectedPort);

                //  Send channel open confirmation message
                this.SendMessage(new ChannelOpenConfirmationMessage(this.RemoteChannelNumber, this.LocalWindowSize, this.LocalPacketSize, this.LocalChannelNumber));
            }
            catch (Exception exp)
            {
                //  Send channel open failure message
                this.SendMessage(new ChannelOpenFailureMessage(this.RemoteChannelNumber, exp.ToString(), 2));

                throw;
            }

            //  Start reading data from the port and send to channel
            while (this._socket != null && this._socket.CanRead())
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
                        //  Zero bytes received when remote host shuts down the connection
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
                    else if (exp.SocketErrorCode == SocketError.ConnectionAborted)
                    {
                        break;
                    }
                    else
                        throw;  // throw any other error
                }
            }

            this.Close();
        }

        partial void OpenSocket(IPAddress connectedHost, uint connectedPort);

        public override void Close()
        {
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

            //  Read data from the channel and send it to the port
            this.InternalSocketSend(data);
        }

        partial void InternalSocketSend(byte[] data);
        
        partial void InternalSocketReceive(byte[] buffer, ref int read);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (this._socket != null)
            {
                this._socket.Dispose();
                this._socket = null;
            }

            base.Dispose(disposing);
        }
    }
}
