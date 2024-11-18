#if NET6_0_OR_GREATER

using System;
using System.Diagnostics;
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

        public static ValueTask SendAsync(Socket socket, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            Debug.Assert(socket != null);
            Debug.Assert(data.Length > 0);

            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromCanceled(cancellationToken);
            }

            return SendAsyncCore(socket, data, cancellationToken);

            static async ValueTask SendAsyncCore(Socket socket, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
            {
                do
                {
                    try
                    {
                        var bytesSent = await socket.SendAsync(data, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                        data = data.Slice(bytesSent);
                    }
                    catch (SocketException ex) when (IsErrorResumable(ex.SocketErrorCode))
                    {
                        // Buffer may be full; attempt a short delay and retry
                        await Task.Delay(30, cancellationToken).ConfigureAwait(false);
                    }
                }
                while (data.Length > 0);
            }
        }
    }
}
#endif // NET6_0_OR_GREATER
