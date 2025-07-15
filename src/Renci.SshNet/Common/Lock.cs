#if !NET9_0_OR_GREATER
using System.Threading;

namespace Renci.SshNet.Common
{
    internal sealed class Lock
    {
        private readonly object _lockObject = new object();

        public bool TryEnter()
        {
            return Monitor.TryEnter(_lockObject);
        }

        public void Exit()
        {
            Monitor.Exit(_lockObject);
        }
    }
}
#endif
