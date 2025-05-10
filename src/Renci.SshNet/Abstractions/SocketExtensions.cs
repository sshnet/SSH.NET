#if !NET
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
        private sealed class AwaitableSocketAsyncEventArgs : SocketAsyncEventArgs, INotifyCompletion
        {
            private static readonly Action SENTINEL = () => { };

            private bool _isCancelled;
            private Action _continuationAction;

            public AwaitableSocketAsyncEventArgs()
            {
                Completed += (sender, e) => SetCompleted();
            }

            public AwaitableSocketAsyncEventArgs ExecuteAsync(Func<SocketAsyncEventArgs, bool> func)
            {
                if (!func(this))
                {
                    SetCompleted();
                }

                return this;
            }

            private void SetCompleted()
            {
                IsCompleted = true;
                var continuation = Interlocked.Exchange(ref _continuationAction, SENTINEL);
                if (continuation is not null)
                {
                    continuation();
                }
            }

            public void SetCancelled()
            {
                _isCancelled = true;
                SetCompleted();
            }

            public AwaitableSocketAsyncEventArgs GetAwaiter()
            {
                return this;
            }

            public bool IsCompleted { get; private set; }

            void INotifyCompletion.OnCompleted(Action continuation)
            {
                if (_continuationAction == SENTINEL || Interlocked.CompareExchange(ref _continuationAction, continuation, comparand: null) == SENTINEL)
                {
                    // We have already completed; run continuation asynchronously
                    _ = Task.Run(continuation);
                }
            }

            public void GetResult()
            {
                if (_isCancelled)
                {
                    throw new TaskCanceledException();
                }

                if (!IsCompleted)
                {
                    // We don't support sync/async
                    throw new InvalidOperationException("The asynchronous operation has not yet completed.");
                }

                if (SocketError != SocketError.Success)
                {
                    throw new SocketException((int)SocketError);
                }
            }
        }

        public static async Task ConnectAsync(this Socket socket, EndPoint remoteEndpoint, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var args = new AwaitableSocketAsyncEventArgs())
            {
                args.RemoteEndPoint = remoteEndpoint;

                using (cancellationToken.Register(o => ((AwaitableSocketAsyncEventArgs)o).SetCancelled(), args, useSynchronizationContext: false))
                {
                    await args.ExecuteAsync(socket.ConnectAsync);
                }
            }
        }

        public static async Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var args = new AwaitableSocketAsyncEventArgs())
            {
                args.SetBuffer(buffer, offset, length);

                using (cancellationToken.Register(o => ((AwaitableSocketAsyncEventArgs)o).SetCancelled(), args, useSynchronizationContext: false))
                {
                    await args.ExecuteAsync(socket.ReceiveAsync);
                }

                return args.BytesTransferred;
            }
        }
    }
}
#endif
