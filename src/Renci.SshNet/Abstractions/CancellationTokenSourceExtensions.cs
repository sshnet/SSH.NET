#if !NET
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Abstractions
{
    internal static class CancellationTokenSourceExtensions
    {
        public static Task CancelAsync(this CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
#endif
