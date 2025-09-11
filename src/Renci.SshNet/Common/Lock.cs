#if !NET9_0_OR_GREATER
using System.Threading;

namespace Renci.SshNet.Common
{
    internal sealed class Lock
    {
        public bool TryEnter()
        {
#pragma warning disable CA2002 // Do not lock on objects with weak identity
            return Monitor.TryEnter(this);
#pragma warning restore CA2002 // Do not lock on objects with weak identity
        }

        public void Exit()
        {
            Monitor.Exit(this);
        }
    }
}
#endif
