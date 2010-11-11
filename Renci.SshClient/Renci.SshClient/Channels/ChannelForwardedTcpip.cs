using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal class ChannelForwardedTcpip : Channel
    {
        private Socket _socket;

        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.ForwardedTcpip; }
        }

        public ChannelForwardedTcpip()
            : base()
        {

        }

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
                this.SendMessage(new ChannelOpenConfirmationMessage
                {
                    LocalChannelNumber = this.RemoteChannelNumber,
                    InitialWindowSize = this.LocalWindowSize,
                    MaximumPacketSize = this.PacketSize,
                    RemoteChannelNumber = this.LocalChannelNumber,
                });
            }
            catch (Exception exp)
            {
                //  Send channel open failure message
                this.SendMessage(new ChannelOpenFailureMessage
                {
                    LocalChannelNumber = this.RemoteChannelNumber,
                    Description = exp.ToString(),
                    ReasonCode = 2,
                });

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
                        this.SendMessage(new ChannelDataMessage
                        {
                            LocalChannelNumber = this.RemoteChannelNumber,
                            Data = buffer.Take(read).GetSshString(),
                        });
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

            this.SendMessage(new ChannelEofMessage
            {
                LocalChannelNumber = this.RemoteChannelNumber,
            });

            this.Close();
        }

        protected override void OnData(string data)
        {
            base.OnData(data);

            //  Read data from the channel and send it to the port
            this._socket.Send(data.GetSshBytes().ToArray());
        }


        protected override void OnDisposing()
        {
            if (this._socket != null)
            {
                this._socket.Close();
            }
        }
    }
}
