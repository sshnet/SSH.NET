using System;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ShellStream
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }
    }
}