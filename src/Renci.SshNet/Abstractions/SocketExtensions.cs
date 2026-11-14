#if !NET
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Abstractions
{
    internal static class SocketExtensions
    {
        public static Task ConnectAsync(this Socket socket, EndPoint remoteEndpoint, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var connectTask = Task.Factory.FromAsync(
                    static (s, remoteEndpoint, callback, state) => s.BeginConnect(remoteEndpoint, callback, state),
                    static result => ((Socket)result.AsyncState).EndConnect(result),
                    socket,
                    remoteEndpoint,
                    state: socket);

            return connectTask.IsCompleted || !cancellationToken.CanBeCanceled
                ? connectTask
                : WaitWithCancellation(connectTask, socket, cancellationToken);
        }

        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            var receiveTask = Task.Factory.FromAsync(
                    static (s, buffer, callback, state) => s.BeginReceive(buffer.Array, buffer.Offset, buffer.Count, SocketFlags.None, callback, state),
                    static result => ((Socket)result.AsyncState).EndReceive(result),
                    socket,
                    new ArraySegment<byte>(buffer, offset, length),
                    state: socket);

            return receiveTask.IsCompleted || !cancellationToken.CanBeCanceled
                ? receiveTask
                : WaitWithCancellation(receiveTask, socket, cancellationToken);
        }

        public static Task<int> SendAsync(this Socket socket, byte[] buffer, CancellationToken cancellationToken)
        {
            return SendAsync(socket, buffer, 0, buffer.Length, cancellationToken);
        }

        public static Task<int> SendAsync(this Socket socket, byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            var sendTask = Task.Factory.FromAsync(
                    static (s, buffer, callback, state) => s.BeginSend(buffer.Array, buffer.Offset, buffer.Count, SocketFlags.None, callback, state),
                    static result => ((Socket)result.AsyncState).EndSend(result),
                    socket,
                    new ArraySegment<byte>(buffer, offset, length),
                    state: socket);

            return sendTask.IsCompleted || !cancellationToken.CanBeCanceled
                ? sendTask
                : WaitWithCancellation(sendTask, socket, cancellationToken);
        }

        private static async Task WaitWithCancellation(Task task, Socket socket, CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(static s => ((Socket)s).Dispose(), socket, useSynchronizationContext: false))
            {
                await task.ConfigureAwait(false);
            }
        }

        private static async Task<int> WaitWithCancellation(Task<int> task, Socket socket, CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(static s => ((Socket)s).Dispose(), socket, useSynchronizationContext: false))
            {
                return await task.ConfigureAwait(false);
            }
        }
    }
}
#endif
