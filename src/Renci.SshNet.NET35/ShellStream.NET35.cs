using System;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents instance of the SSH shell object
    /// </summary>
    public partial class ShellStream
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }
    }
}
