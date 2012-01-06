using System;
using System.Threading.Tasks;

namespace Renci.SshNet
{
    public partial class ForwardedPortDynamic
    {
        partial void ExecuteThread(Action action)
        {
            Task.Factory.StartNew(action);
        }
    }
}
