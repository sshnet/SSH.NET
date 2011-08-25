using System;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents instance of the SSH shell object
    /// </summary>
    public partial class Shell
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }
    }
}
