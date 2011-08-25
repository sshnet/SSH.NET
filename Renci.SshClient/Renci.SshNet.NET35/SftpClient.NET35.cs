using System;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SftpClient
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }
    }
}