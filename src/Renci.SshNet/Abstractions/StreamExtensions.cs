#if !NET && !NETSTANDARD2_1_OR_GREATER
using System.IO;
using System.Threading.Tasks;
#endif

namespace Renci.SshNet.Abstractions
{
    internal static class StreamExtensions
    {
#if !NET && !NETSTANDARD2_1_OR_GREATER
        public static ValueTask DisposeAsync(this Stream stream)
        {
            stream.Dispose();
            return default;
        }
#endif
    }
}
