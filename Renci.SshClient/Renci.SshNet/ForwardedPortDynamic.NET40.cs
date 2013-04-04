using System;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet
{
    public partial class ForwardedPortDynamic
    {
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }
    }
}
