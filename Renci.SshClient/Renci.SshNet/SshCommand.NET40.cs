using System;
using System.Threading.Tasks;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents SSH command that can be executed.
    /// </summary>
    public partial class SshCommand 
    {
        partial void ExecuteThread(Action action)
        {
            Task.Factory.StartNew(action);
        }
    }
}
