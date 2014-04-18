using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using System.Text;

namespace Renci.SshNet
{
    public partial class Session
    {
        private readonly AutoResetEvent _autoEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _sendEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _receiveEvent = new AutoResetEvent(false);

        private bool _isConnected;

        partial void IsSocketConnected(ref bool isConnected)
        {
            isConnected = (this._socket != null && this._socket.Connected && _isConnected);
        }

        partial void SocketConnect(string host, int port)
        {
            var ep = new DnsEndPoint(host, port);
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var args = new SocketAsyncEventArgs();
            args.UserToken = this._socket;
            args.RemoteEndPoint = ep;
            args.Completed += OnConnect;

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

            var result = new StringBuilder();

            do
            {
                var args = new SocketAsyncEventArgs();
                args.SetBuffer(buffer, 0, buffer.Length);
                args.UserToken = this._socket;
                args.RemoteEndPoint = this._socket.RemoteEndPoint;
                args.Completed += OnReceive;
                this._socket.ReceiveAsync(args);

                this._receiveEvent.WaitOne(this.ConnectionInfo.Timeout);

                var lastChar = (char)buffer[0];
                for (var i = 1; i < args.BytesTransferred; i++)
                {
                    var newChar = (char)buffer[i];
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
            var receivedTotal = 0;  // how many bytes is already received

            do
            {
                var args = new SocketAsyncEventArgs();
                args.SetBuffer(buffer, receivedTotal, length - receivedTotal);
                args.UserToken = this._socket;
                args.RemoteEndPoint = this._socket.RemoteEndPoint;
                args.Completed += OnReceive;
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
                throw new SshConnectionException(
                    "An established connection was aborted by the software in your host machine.",
                    DisconnectReason.ConnectionLost);
            } while (receivedTotal < length);
        }

        partial void SocketWrite(byte[] data)
        {
            if (this._isConnected)
            {
                var args = new SocketAsyncEventArgs();
                args.SetBuffer(data, 0, data.Length);
                args.UserToken = this._socket;
                args.RemoteEndPoint = this._socket.RemoteEndPoint;
                args.Completed += OnSend;

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
            ThreadPool.QueueUserWorkItem(o => action());
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
