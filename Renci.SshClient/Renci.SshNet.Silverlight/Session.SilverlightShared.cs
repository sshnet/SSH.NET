using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using System.Text;
using Renci.SshNet.Messages;
using System.Collections.Generic;

namespace Renci.SshNet
{
    public partial class Session
    {
        private AutoResetEvent _autoEvent = new AutoResetEvent(false);
        private AutoResetEvent _sendEvent = new AutoResetEvent(false);
        private AutoResetEvent _receiveEvent = new AutoResetEvent(false);

        private bool _isConnected = false;

        partial void IsSocketConnected(ref bool isConnected)
        {
            isConnected = (!this._isDisconnecting && this._socket != null && this._socket.Connected && this._isAuthenticated && this._messageListenerCompleted != null && this._isConnected);
        }

        partial void SocketConnect(string host, int port)
        {
            var ep = new DnsEndPoint(host, port);
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();

            args.UserToken = this._socket;
            args.RemoteEndPoint = ep;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);

            this._socket.ConnectAsync(args);
            this._autoEvent.WaitOne(this.ConnectionInfo.Timeout);

            if (args.SocketError != SocketError.Success)
                throw new SocketException((int)args.SocketError);
        }

        partial void SocketDisconnect()
        {
            this._socket.Close(10000);
        }

        partial void SocketReadLine(ref string response)
        {
            var encoding = new Renci.SshNet.Common.ASCIIEncoding();

            var line = new StringBuilder();
            //  Read data one byte at a time to find end of line and leave any unhandled information in the buffer to be processed later
            var buffer = new List<byte>();

            var data = new byte[1];
            do
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(data, 0, data.Length);
                args.UserToken = this._socket;
                args.RemoteEndPoint = this._socket.RemoteEndPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                this._socket.ReceiveAsync(args);

                if (!this._receiveEvent.WaitOne(this.ConnectionInfo.Timeout))
                    throw new SshOperationTimeoutException("Socket read operation has timed out");

                //  If zero bytes received then exit
                if (args.BytesTransferred == 0)
                    break;

                buffer.Add(data[0]);
            }
            while (!(buffer.Count > 0 && (buffer[buffer.Count - 1] == 0x0A || buffer[buffer.Count - 1] == 0x00)));

            // Return an empty version string if the buffer consists of a 0x00 character.
            if (buffer.Count > 0 && buffer[buffer.Count - 1] == 0x00)
            {
                response = string.Empty;
            }
            else if (buffer.Count == 0) 
                response = string.Empty;
            else if (buffer.Count > 1 && buffer[buffer.Count - 2] == 0x0D)
                response = encoding.GetString(buffer.ToArray(), 0, buffer.Count - 2);
            else
                response = encoding.GetString(buffer.ToArray(), 0, buffer.Count - 1);
        }

        partial void SocketRead(int length, ref byte[] buffer)
        {
            var offset = 0;
            int receivedTotal = 0;  // how many bytes is already received

            do
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(buffer, offset + receivedTotal, length - receivedTotal);
                args.UserToken = this._socket;
                args.RemoteEndPoint = this._socket.RemoteEndPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                this._socket.ReceiveAsync(args);

                this._receiveEvent.WaitOne(this.ConnectionInfo.Timeout);

                if (args.SocketError == SocketError.WouldBlock ||
                    args.SocketError == SocketError.IOPending ||
                    args.SocketError == SocketError.NoBufferSpaceAvailable)
                {
                    // socket buffer is probably empty, wait and try again
                    Thread.Sleep(30);
                    continue;
                }
                else if (args.SocketError != SocketError.Success)
                {
                    throw new SocketException((int)args.SocketError);
                }

                var receivedBytes = args.BytesTransferred;

                if (receivedBytes > 0)
                {
                    receivedTotal += receivedBytes;
                    continue;
                }
                else
                {
                    throw new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReason.ConnectionLost);
                }
            } while (receivedTotal < length);
        }

        partial void SocketWrite(byte[] data)
        {
            if (this._isConnected)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(data, 0, data.Length);
                args.UserToken = this._socket;
                args.RemoteEndPoint = this._socket.RemoteEndPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);

                this._socket.SendAsync(args);
            }
            else
                throw new SocketException((int)SocketError.NotConnected);

        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            this._autoEvent.Set();
            this._isConnected = (e.SocketError == SocketError.Success);
        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            this._sendEvent.Set();
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            this._receiveEvent.Set();
        }

        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }

        partial void InternalRegisterMessage(string messageName)
        {
            lock (this._messagesMetadata)
            {
                foreach (var item in from m in this._messagesMetadata where m.Name == messageName select m)
                {
                    item.Enabled = true;
                    item.Activated = true;
                }
            }
        }

        partial void InternalUnRegisterMessage(string messageName)
        {
            lock (this._messagesMetadata)
            {
                foreach (var item in from m in this._messagesMetadata where m.Name == messageName select m)
                {
                    item.Enabled = false;
                    item.Activated = false;
                }
            }
        }

    }
}
