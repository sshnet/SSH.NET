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
#elif FEATURE_THREAD_TAP
            System.Threading.Tasks.Task.Delay(millisecondsTimeout).GetAwaiter().GetResult();
#else
            #error Suspend of the current thread is not implemented.
#endif
        }

        public static void ExecuteThreadLongRunning(Action action)
        {
#if FEATURE_THREAD_TAP
            var taskCreationOptions = System.Threading.Tasks.TaskCreationOptions.LongRunning;
            System.Threading.Tasks.Task.Factory.StartNew(action, taskCreationOptions);
#else
            var thread = new System.Threading.Thread(() => action());
            thread.Start();
#endif
        }

        /// <summary>
        /// Executes the specified action in a separate thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void ExecuteThread(Action action)
        {
#if FEATURE_THREAD_THREADPOOL
            if (action == null)
                throw new ArgumentNullException("action");

            System.Threading.ThreadPool.QueueUserWorkItem(o => action());
#elif FEATURE_THREAD_TAP
            System.Threading.Tasks.Task.Run(action);
#else
            #error Execution of action in a separate thread is not implemented.
#endif
        }
    }
}
