using System;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides connection information when password authentication method is used
    /// </summary>
    public partial class PasswordConnectionInfo
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }
    }
}
