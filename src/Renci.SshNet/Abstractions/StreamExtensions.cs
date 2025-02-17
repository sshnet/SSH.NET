#if NETFRAMEWORK || NETSTANDARD2_0
using System.IO;
using System.Threading.Tasks;

namespace Renci.SshNet.Abstractions
{
    internal static class StreamExtensions
    {
        public static ValueTask DisposeAsync(this Stream stream)
        {
            stream.Dispose();
            return default;
        }
    }
}
#endif
