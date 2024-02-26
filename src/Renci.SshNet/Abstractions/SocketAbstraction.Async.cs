#if NET6_0_OR_GREATER

using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Abstractions
{
    internal static partial class SocketAbstraction
    {
        public static ValueTask<int> ReadAsync(Socket socket, byte[] buffer, CancellationToken cancellationToken)
        {
            return socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }
    }
}
#endif // NET6_0_OR_GREATER
