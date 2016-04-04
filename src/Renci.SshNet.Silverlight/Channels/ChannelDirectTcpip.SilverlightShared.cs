using System.Net.Sockets;
using System.Threading;

namespace Renci.SshNet.Channels
{
    internal partial class ChannelDirectTcpip
    {
        private readonly AutoResetEvent _sendEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _receiveEvent = new AutoResetEvent(false);

        partial void InternalSocketReceive(byte[] buffer, ref int read)
        {
            var bytesToRead = buffer.Length;
            var receivedTotal = 0;  // how many bytes is already received

            do
            {
                var args = new SocketAsyncEventArgs();
                args.SetBuffer(buffer, receivedTotal, bytesToRead - receivedTotal);
                args.UserToken = _socket;
                args.RemoteEndPoint = _socket.RemoteEndPoint;
                args.Completed += OnReceive;
                _socket.ReceiveAsync(args);

                _receiveEvent.WaitOne(ConnectionInfo.Timeout);

                if (args.SocketError == SocketError.WouldBlock ||
                    args.SocketError == SocketError.IOPending ||
                    args.SocketError == SocketError.NoBufferSpaceAvailable)
                {
                    // socket buffer is probably empty, wait and try again
                    Thread.Sleep(30);
                    continue;
                }

                if (args.SocketError != SocketError.Success)
                {
                    throw new SocketException((int)args.SocketError);
                }

                var receivedBytes = args.BytesTransferred;
                if (receivedBytes > 0)
                {
                    receivedTotal += receivedBytes;
                    continue;
                }
                break;
            } while (receivedTotal < bytesToRead);

            read = receivedTotal;
        }

        partial void InternalSocketSend(byte[] data)
        {
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(data, 0, data.Length);
            args.UserToken = _socket;
            args.RemoteEndPoint = _socket.RemoteEndPoint;
            args.Completed += OnSend;
            _socket.SendAsync(args);
            _sendEvent.WaitOne(ConnectionInfo.Timeout);
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            _receiveEvent.Set();
        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            _sendEvent.Set();
        }
    }
}
