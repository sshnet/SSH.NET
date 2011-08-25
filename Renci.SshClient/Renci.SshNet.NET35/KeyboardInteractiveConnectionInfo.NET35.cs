using System;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides connection information when keyboard interactive authentication method is used
    /// </summary>
    public partial class KeyboardInteractiveConnectionInfo
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }
    }
}
