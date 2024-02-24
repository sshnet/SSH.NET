#if !NET
#if NETFRAMEWORK || NETSTANDARD2_0
using System;
#endif
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Abstractions
{
    internal static class SocketExtensions
    {
        public static async Task ConnectAsync(this Socket socket, IPEndPoint remoteEndpoint, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TaskCompletionSource<object> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            using var args = new SocketAsyncEventArgs
            {
                RemoteEndPoint = remoteEndpoint
            };
            args.Completed += (_, _) => tcs.TrySetResult(null);

            if (socket.ConnectAsync(args))
            {
#if NETSTANDARD2_1
                await using (cancellationToken.Register(() =>
#else
                using (cancellationToken.Register(() =>
#endif
                {
                    if (tcs.TrySetCanceled(cancellationToken))
                    {
                        socket.Dispose();
                    }
                },
                useSynchronizationContext: false)
#if NETSTANDARD2_1
                .ConfigureAwait(false)
#endif
                )
                {
                    _ = await tcs.Task.ConfigureAwait(false);
                }
            }

            if (args.SocketError != SocketError.Success)
            {
                throw new SocketException((int) args.SocketError);
            }
        }

#if NETFRAMEWORK || NETSTANDARD2_0
        public static async ValueTask<int> ReceiveAsync(this Socket socket, ArraySegment<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TaskCompletionSource<object> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            using var args = new SocketAsyncEventArgs();
            args.SocketFlags = socketFlags;
            args.Completed += (_, _) => tcs.TrySetResult(null);
            args.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);

            if (socket.ReceiveAsync(args))
            {
                using (cancellationToken.Register(() =>
                {
                    if (tcs.TrySetCanceled(cancellationToken))
                    {
                        socket.Dispose();
                    }
                },
                useSynchronizationContext: false))
                {
                    _ = await tcs.Task.ConfigureAwait(false);
                }
            }

            if (args.SocketError != SocketError.Success)
            {
                throw new SocketException((int) args.SocketError);
            }

            return args.BytesTransferred;
        }

        public static async ValueTask<int> SendAsync(this Socket socket, byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TaskCompletionSource<object> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            using var args = new SocketAsyncEventArgs();
            args.SocketFlags = socketFlags;
            args.Completed += (_, _) => tcs.TrySetResult(null);
            args.SetBuffer(buffer, 0, buffer.Length);

            if (socket.SendAsync(args))
            {
                using (cancellationToken.Register(() =>
                {
                    if (tcs.TrySetCanceled(cancellationToken))
                    {
                        socket.Dispose();
                    }
                },
                useSynchronizationContext: false))
                {
                    _ = await tcs.Task.ConfigureAwait(false);
                }
            }

            if (args.SocketError != SocketError.Success)
            {
                throw new SocketException((int) args.SocketError);
            }

            return args.BytesTransferred;
        }
#endif // NETFRAMEWORK || NETSTANDARD2_0
    }
}
#endif
