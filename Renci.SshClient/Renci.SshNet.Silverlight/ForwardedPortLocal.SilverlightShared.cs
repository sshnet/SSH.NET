using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Renci.SshNet.Channels;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public partial class ForwardedPortLocal
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }

        partial void InternalStart()
        {
            throw new NotImplementedException();
        }

        partial void InternalStop()
        {
            throw new NotImplementedException();
        }
    }
}
