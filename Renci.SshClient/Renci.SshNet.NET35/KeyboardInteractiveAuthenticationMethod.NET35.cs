using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
