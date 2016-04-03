using System;

namespace Renci.SshNet.Abstractions
{
    internal static class ThreadAbstraction
    {
        /// <summary>
        /// Suspends the current thread for the specified number of milliseconds.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds for which the thread is suspended.</param>
        public static void Sleep(int millisecondsTimeout)
        {
#if FEATURE_THREAD_SLEEP
            System.Threading.Thread.Sleep(millisecondsTimeout);
#elif FEATURE_TPL
            System.Threading.Tasks.Task.Delay(millisecondsTimeout).Wait();
#else
#error Suspend of the current thread is not implemented.
#endif
        }

        /// <summary>
        /// Executes the specified action in a separate thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void ExecuteThread(Action action)
        {
#if FEATURE_TPL
            System.Threading.Tasks.Task.Run(action);
#else
            System.Threading.ThreadPool.QueueUserWorkItem(o => action());
#endif
        }
    }
}
