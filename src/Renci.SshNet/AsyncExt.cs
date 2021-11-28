using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// 
    /// </summary>
    public static class AsyncExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return handle.WaitOneAsync((int)timeout.TotalMilliseconds, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken cancellationToken)
        {
            return handle.WaitOneAsync(Timeout.Infinite, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="timeoutInMs"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> WaitOneAsync(this WaitHandle handle, int timeoutInMs, CancellationToken cancellationToken)
        {
            RegisteredWaitHandle registeredHandle = null;
            CancellationTokenRegistration reg = default(CancellationTokenRegistration);

            try
            {
                var tcs = new TaskCompletionSource<bool>();
                registeredHandle = ThreadPool.RegisterWaitForSingleObject(
                    handle,
                    (state, timedout) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedout),
                    tcs,
                    timeoutInMs,
                    true);

                reg = cancellationToken.Register(
                    state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
                    tcs);
                return await tcs.Task;
            }
            finally
            {
                if (registeredHandle != null)
                {
                    registeredHandle.Unregister(null);
                }
                reg.Dispose();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<int> WaitAnyAsync(this WaitHandle[] handle, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return handle.WaitAnyAsync((int)timeout.TotalMilliseconds, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<int> WaitAnyAsync(this WaitHandle[] handle, CancellationToken cancellationToken)
        {
            return handle.WaitAnyAsync(Timeout.Infinite, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handles"></param>
        /// <param name="timeoutInMs"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<int> WaitAnyAsync(this WaitHandle[] handles, int timeoutInMs, CancellationToken cancellationToken)
        {
            if (handles.Length <= 0)
                throw new ArgumentException();

            List<Task> tasks = new List<Task>();
            var registrations = new List<(RegisteredWaitHandle, CancellationTokenRegistration)>();

            try
            {
                foreach (var handle in handles)
                {
                    RegisteredWaitHandle registeredHandle = null;
                    CancellationTokenRegistration reg = default(CancellationTokenRegistration);

                    var tcs = new TaskCompletionSource<bool>();
                    registeredHandle = ThreadPool.RegisterWaitForSingleObject(
                        handle,
                        (state, timedout) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedout),
                        tcs,
                        timeoutInMs,
                        true);

                    reg = cancellationToken.Register(
                        state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
                        tcs);

                    tasks.Add(tcs.Task);
                    registrations.Add((registeredHandle, reg));
                }

                return await Task.FromResult<int>(Task.WaitAny(tasks.ToArray(), timeoutInMs, cancellationToken));
            }
            finally
            {
                foreach (var (registeredHandle, reg) in registrations)
                {
                    if (registeredHandle != null)
                    {
                        registeredHandle.Unregister(null);
                    }
                    reg.Dispose();
                }
            }
        }
    }
}
