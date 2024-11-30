#if !NET9_0_OR_GREATER
using System.Threading;

namespace Renci.SshNet.Common
{
    internal sealed class Lock
    {
        public bool TryEnter()
        {
            return Monitor.TryEnter(this);
        }

        public void Exit()
        {
            Monitor.Exit(this);
        }
    }
}
#endif
