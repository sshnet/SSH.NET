#nullable enable
using System;

namespace Renci.SshNet.Common
{
    internal static class ThrowHelper
    {
        public static void ThrowObjectDisposedIf(bool condition, object instance)
        {
#if NET7_0_OR_GREATER
            ObjectDisposedException.ThrowIf(condition, instance);
#else
            if (condition)
            {
                Throw(instance);

                static void Throw(object? instance)
                {
                    throw new ObjectDisposedException(instance?.GetType().FullName);
                }
            }
#endif
        }
    }
}
