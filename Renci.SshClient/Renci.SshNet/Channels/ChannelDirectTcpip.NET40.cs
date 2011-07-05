using System.Threading.Tasks;
using System;
using System.Net.Sockets;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Implements "direct-tcpip" SSH channel.
    /// </summary>
    internal partial class ChannelDirectTcpip 
    {
        partial void ExecuteThread(Action action)
        {
            Task.Factory.StartNew(action, TaskCreationOptions.LongRunning);
        }

        partial void InternalSocketReceive(byte[] buffer, ref int read)
        {
            read = this._socket.Receive(buffer);
        }

        partial void InternalSocketSend(byte[] data)
        {
            this._socket.Send(data, 0, data.Length, SocketFlags.None);
        }
    }
}
