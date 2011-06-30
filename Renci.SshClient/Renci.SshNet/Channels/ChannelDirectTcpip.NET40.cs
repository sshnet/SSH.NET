using System.Threading.Tasks;
using System;

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
    }
}
