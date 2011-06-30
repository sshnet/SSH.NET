using System.Threading.Tasks;
using System;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public partial class ForwardedPortLocal 
    {
        partial void ExecuteThread(Action action)
        {
            Task.Factory.StartNew(action);
        }
    }
}
