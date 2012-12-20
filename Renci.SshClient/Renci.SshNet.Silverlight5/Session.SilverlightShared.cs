using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using System.Text;
using Renci.SshNet.Messages;

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
            //  TODO:   Improve this function, currently will not work with server that send multiple lines as a first string

            var buffer = new byte[1024];

            StringBuilder result = new StringBuilder();

            do
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(buffer, 0, buffer.Length);
                args.UserToken = this._socket;
                args.RemoteEndPoint = this._socket.RemoteEndPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                this._socket.ReceiveAsync(args);

                this._receiveEvent.WaitOne(this.ConnectionInfo.Timeout);

                char lastChar = (char)buffer[0];
                for (int i = 1; i < args.BytesTransferred; i++)
                {
                    char newChar = (char)buffer[i];
                    if (lastChar == '\r' && newChar == '\n')
                        break;

                    result.Append(lastChar);
                    lastChar = newChar;
                }

                if (args.BytesTransferred < buffer.Length)
                    break;

            } while (true);

            response = result.ToString();
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
