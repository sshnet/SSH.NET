using System;
using System.Threading;

namespace Renci.SshNet
{
    public partial  class KeyboardInteractiveAuthenticationMethod : AuthenticationMethod
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem(o => action());
        }
    }
}
