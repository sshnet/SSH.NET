using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshClient.Common;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal class ChannelDirectTcpip : Channel
    {
        public EventWaitHandle _channelEof = new AutoResetEvent(false);

        private EventWaitHandle _channelOpen = new AutoResetEvent(false);

        private EventWaitHandle _channelData = new AutoResetEvent(false);

        private Socket _socket;

        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.DirectTcpip; }
        }

        public ChannelDirectTcpip()
            : base()
        {

        }

        public void Bind(string remoteHost, uint port, Socket socket)
        {
            this._socket = socket;

            IPEndPoint ep = socket.RemoteEndPoint as IPEndPoint;

            if (!this.Session.IsConnected)
            {
                throw new SshException("Session is not connected.");
            }

            //  Open channel
            this.SendMessage(new ChannelOpenDirectTcpIPMessage
            {
                ChannelType = ChannelTypes.DirectTcpip,
                LocalChannelNumber = this.LocalChannelNumber,
                InitialWindowSize = this.LocalWindowSize,
                MaximumPacketSize = this.PacketSize,
                HostToConnect = remoteHost,
                PortToConnect = port,
                OriginatorIP = "0.0.0.0",
                OriginatorPort = 0
            });

            //  Wait for channel to open
            this.Session.WaitHandle(this._channelOpen);

            //  Start reading data from the port and send to channel
            EventWaitHandle readerTaskError = new AutoResetEvent(false);

            var readerTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    var buffer = new byte[this.PacketSize - 9];

                    while (this._socket.Connected || this.Session.IsConnected)
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
                }
                catch (Exception exp)
                {
                    readerTaskError.Set();
                    throw;
                }
            });

            //  Channel was open and we MUST receive EOF notification, 
            //  data transfer can take longer then connection specified timeout
            WaitHandle.WaitAny(new WaitHandle[] { this._channelEof, readerTaskError });

            this._socket.Close();

            //  Wait for task to finish and will throw any errors if any
            readerTask.Wait();
        }

        protected override void OnChannelData(string data)
        {
            base.OnChannelData(data);

            this._socket.Send(data.GetSshBytes().ToArray(), 0, data.Length, SocketFlags.None);
        }

        protected override void OnChannelOpen()
        {
            base.OnChannelOpen();

            this._channelOpen.Set();
        }

        protected override void OnChannelEof()
        {
            base.OnChannelEof();

            this._channelEof.Set();
        }

        protected override void OnChannelClose()
        {
            base.OnChannelClose();
        }

        protected override void OnDisposing()
        {

        }
    }
}
