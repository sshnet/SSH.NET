using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Abstractions
{
    internal static class CancellationTokenSourceExtensions
    {
#if !NET8_OR_GREATER
        public static Task CancelAsync(this CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
#endif
    }
}
