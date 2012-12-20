using System;
using System.Threading;

namespace Renci.SshNet
{
    public partial class PasswordAuthenticationMethod
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }
    }
}
