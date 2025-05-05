#if NETFRAMEWORK || NETSTANDARD2_0
using System;
using System.IO;
using System.Threading.Tasks;

namespace Renci.SshNet.Abstractions
{
    internal static class StreamExtensions
    {
        public static ValueTask DisposeAsync(this Stream stream)
        {
            try
            {
                stream.Dispose();
                return default;
            }
            catch (Exception exc)
            {
                return new ValueTask(Task.FromException(exc));
            }
        }
    }
}
#endif
