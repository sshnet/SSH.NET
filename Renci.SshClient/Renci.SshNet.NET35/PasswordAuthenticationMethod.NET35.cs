using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
