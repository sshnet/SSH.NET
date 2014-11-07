using System.Net;
using System.Net.Sockets;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Implements "forwarded-tcpip" SSH channel.
    /// </summary>
    internal partial class ChannelForwardedTcpip
    {
        partial void OpenSocket(IPEndPoint remoteEndpoint)
        {
            this._socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this._socket.Connect(remoteEndpoint);
            this._socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
        }

        partial void InternalSocketReceive(byte[] buffer, ref int read)
        {
            read = this._socket.Receive(buffer);
        }

        partial void InternalSocketSend(byte[] data)
        {
            this._socket.Send(data);
        }
    }
}
