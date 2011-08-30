using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
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
        private AutoResetEvent autoEvent = new AutoResetEvent(false);
        private AutoResetEvent sendEvent = new AutoResetEvent(false);
        private AutoResetEvent receiveEvent = new AutoResetEvent(false);

        private bool isConnected = false;

        partial void SocketConnect()
        {
            var ep = new DnsEndPoint(this.ConnectionInfo.Host, this.ConnectionInfo.Port);
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();

            args.UserToken = this._socket;
            args.RemoteEndPoint = ep;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);

            this._socket.ConnectAsync(args);
            autoEvent.WaitOne(this.ConnectionInfo.Timeout);

            if (args.SocketError != SocketError.Success)
                throw new SocketException((int)args.SocketError);
        }

        partial void SocketDisconnect()
        {
            this._socket.Close(10000);
        }

        partial void SocketReadLine(ref string response)
        {
            var buffer = new byte[1024];
            StringBuilder sb = new StringBuilder();

            do
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(buffer, 0, buffer.Length);
                args.UserToken = this._socket;
                args.RemoteEndPoint = this._socket.RemoteEndPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                this._socket.ReceiveAsync(args);

                this.receiveEvent.WaitOne(this.ConnectionInfo.Timeout);

                sb.Append(Encoding.UTF8.GetString(buffer, 0, args.BytesTransferred));

                if (args.BytesTransferred < buffer.Length)
                    break;

            } while (true);

            response = sb.ToString();

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

                    this.receiveEvent.WaitOne(this.ConnectionInfo.Timeout);

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
            if (isConnected)
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
            autoEvent.Set();
            isConnected = (e.SocketError == SocketError.Success);
        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            this.sendEvent.Set();
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            this.receiveEvent.Set();
        }

        partial void HandleMessageCore(Message message)
        {
            this.HandleMessage((dynamic)message);
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
