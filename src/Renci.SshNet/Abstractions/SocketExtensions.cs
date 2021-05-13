#if FEATURE_TAP
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Abstractions
{
    // Async helpers based on https://devblogs.microsoft.com/pfxteam/awaiting-socket-operations/

    internal static class SocketExtensions
    {
        sealed class SocketAsyncEventArgsAwaitable : SocketAsyncEventArgs, INotifyCompletion
        {
            private readonly static Action SENTINEL = () => { };

            private bool isCancelled;
            private Action continuationAction;

            public SocketAsyncEventArgsAwaitable()
            {
                Completed += delegate { SetCompleted(); };
            }

            public SocketAsyncEventArgsAwaitable ExecuteAsync(Func<SocketAsyncEventArgs, bool> func)
            {
                if (!func(this))
                {
                    SetCompleted();
                }
                return this;
            }

            public void SetCompleted()
            {
                IsCompleted = true;
                var continuation = continuationAction ?? Interlocked.CompareExchange(ref continuationAction, SENTINEL, null);
                if (continuation != null)
                {
                    continuation();
                }
            }

            public void SetCancelled()
            {
                isCancelled = true;
                SetCompleted();
            }

            public SocketAsyncEventArgsAwaitable GetAwaiter() { return this; }

            public bool IsCompleted { get; private set; }

            void INotifyCompletion.OnCompleted(Action continuation)
            {
                if (continuationAction == SENTINEL || Interlocked.CompareExchange(ref continuationAction, continuation, null) == SENTINEL)
                {
                    // We have already completed; run continuation asynchronously
                    Task.Run(continuation);
                }
            }

            public void GetResult()
            {
                if (isCancelled)
                {
                    throw new TaskCanceledException();
                }
                else if (IsCompleted)
                {
                    if (SocketError != SocketError.Success)
                    {
                        throw new SocketException((int)SocketError);
                    }
                }
                else
                {
                    // We don't support sync/async
                    throw new InvalidOperationException("The asynchronous operation has not yet completed.");
                }
            }
        }

        public static async Task ConnectAsync(this Socket socket, IPEndPoint remoteEndpoint, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var args = new SocketAsyncEventArgsAwaitable())
            {
                args.RemoteEndPoint = remoteEndpoint;

                using (cancellationToken.Register(o => ((SocketAsyncEventArgsAwaitable)o).SetCancelled(), args, false))
                {
                    await args.ExecuteAsync(socket.ConnectAsync);
                }
            }
        }

        public static async Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var args = new SocketAsyncEventArgsAwaitable())
            {
                args.SetBuffer(buffer, offset, length);

                using (cancellationToken.Register(o => ((SocketAsyncEventArgsAwaitable)o).SetCancelled(), args, false))
                {
                    await args.ExecuteAsync(socket.ReceiveAsync);
                }

                return args.BytesTransferred;
            }
        }
    }
}
#endif