using System;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Abstractions
{
    internal static class ThreadAbstraction
    {
        /// <summary>
        /// Creates and starts a long-running <see cref="Task"/> for the specified <see cref="Action"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to start.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
        /// <returns>
        /// A task that represents the execution of the specified <see cref="Action"/>.
        /// </returns>
        public static Task ExecuteThreadLongRunning(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Task.Factory.StartNew(action,
                                         CancellationToken.None,
                                         TaskCreationOptions.LongRunning,
                                         TaskScheduler.Current);
        }

        /// <summary>
        /// Executes the specified action in a separate thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void ExecuteThread(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _ = ThreadPool.QueueUserWorkItem(o => action());
        }
    }
}
