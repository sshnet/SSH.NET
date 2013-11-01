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
    internal partial class ChannelForwardedTcpip : Channel
    {
        partial void OpenSocket(IPAddress connectedHost, uint connectedPort)
        {
            var ep = new IPEndPoint(connectedHost, (int)connectedPort);
            this._socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this._socket.Connect(ep);
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
