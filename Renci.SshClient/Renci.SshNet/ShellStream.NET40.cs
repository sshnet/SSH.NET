using System;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents instance of the SSH shell object
    /// </summary>
    public partial class ShellStream
    {
        /// <exception cref="ArgumentNullException"><paramref name=" action"/> is null.</exception>
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
        }
    }
}
