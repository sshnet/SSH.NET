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
    internal class ChannelForwardedTcpip : Channel
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
            : base()
        {

        }

        /// <summary>
        /// Binds channel to specified connected host.
        /// </summary>
        /// <param name="connectedHost">The connected host.</param>
        /// <param name="connectedPort">The connected port.</param>
        public void Bind(string connectedHost, uint connectedPort)
        {
            byte[] buffer = null;

            this.ServerWindowSize = this.LocalWindowSize;

            if (!this.IsConnected)
            {
                throw new SshException("Session is not connected.");
            }

            //  Try to connect to the socket 
            try
            {
                //  Get buffer in memory for data exchange
                buffer = new byte[this.PacketSize - 9];

                var ep = new IPEndPoint(Dns.GetHostEntry(connectedHost).AddressList[0], (int)connectedPort);
                this._socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this._socket.Connect(ep);
                this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);

                //  Send channel open confirmation message
                this.SendMessage(new ChannelOpenConfirmationMessage(this.RemoteChannelNumber, this.LocalWindowSize, this.PacketSize, this.LocalChannelNumber));
            }
            catch (Exception exp)
            {
                //  Send channel open failure message
                this.SendMessage(new ChannelOpenFailureMessage(this.RemoteChannelNumber, exp.ToString(), 2));

                throw;
            }

            //  Start reading data from the port and send to channel
            while (this._socket.Connected || this.IsConnected)
            {
                try
                {
                    var read = this._socket.Receive(buffer);
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

            this.SendMessage(new ChannelEofMessage(this.RemoteChannelNumber));

            this.Close();
        }

        /// <summary>
        /// Called when channel data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected override void OnData(byte[] data)
        {
            base.OnData(data);

            //  Read data from the channel and send it to the port
            this._socket.Send(data);
        }

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
