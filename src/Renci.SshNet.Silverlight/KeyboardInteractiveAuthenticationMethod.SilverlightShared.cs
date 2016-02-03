using System;
using System.Net;
using System.Threading;

namespace Renci.SshNet
{
    public partial class KeyboardInteractiveAuthenticationMethod
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }
    }
}
