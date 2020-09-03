using System;

namespace Renci.SshNet.Security.Chaos.NaCl.Internal
{
    internal static class InternalAssert
    {
        internal static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("An assertion in Chaos.Crypto failed " + message);
        }
    }
}
