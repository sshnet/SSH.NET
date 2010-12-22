﻿using System;
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

            if (!this.IsConnected)
            {
                throw new SshException("Session is not connected.");
            }

            //  Open channel
            this.SendMessage(new ChannelOpenMessage(this.LocalChannelNumber, this.LocalWindowSize, this.PacketSize,
                                                        new DirectTcpipChannelInfo(remoteHost, port, "0.0.0.0", 0)));

            //  Wait for channel to open
            this.WaitHandle(this._channelOpen);

            //  Start reading data from the port and send to channel
            EventWaitHandle readerTaskError = new AutoResetEvent(false);

            var readerTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    var buffer = new byte[this.PacketSize - 9];

                    while (this._socket.Connected || this.IsConnected)
                    {
                        try
                        {

                            var read = this._socket.Receive(buffer);
                            if (read > 0)
                            {
                                this.SendMessage(new ChannelDataMessage(this.RemoteChannelNumber, buffer.Take(read).GetSshString()));
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
                catch (Exception)
                {
                    readerTaskError.Set();
                    throw;
                }
            });

            //  Channel was open and we MUST receive EOF notification, 
            //  data transfer can take longer then connection specified timeout
            System.Threading.WaitHandle.WaitAny(new WaitHandle[] { this._channelEof, readerTaskError });

            this._socket.Close();

            //  Wait for task to finish and will throw any errors if any
            readerTask.Wait();
        }

        protected override void OnData(string data)
        {
            base.OnData(data);

            this._socket.Send(data.GetSshBytes().ToArray(), 0, data.Length, SocketFlags.None);
        }

        protected override void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            base.OnOpenConfirmation(remoteChannelNumber, initialWindowSize, maximumPacketSize);

            this._channelOpen.Set();
        }

        protected override void OnEof()
        {
            base.OnEof();

            this._channelEof.Set();
        }
    }
}
