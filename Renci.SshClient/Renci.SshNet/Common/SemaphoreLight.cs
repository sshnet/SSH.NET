using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Renci.SshNet.Common
{
    public class SemaphoreLight
    {
        private object _lock = new object();

        private int _currentCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphoreLight"/> class, specifying 
        /// the initial number of requests that can be granted concurrently.
        /// </summary>
        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
        public SemaphoreLight(int initialCount)
        {
            if (initialCount < 0 )
                throw new ArgumentOutOfRangeException("The initial argument is negative");

            this._currentCount = initialCount;
        }

        /// <summary>
        /// Gets the current count of the <see cref="SemaphoreLight"/>.
        /// </summary>
        public int CurrentCount { get { return this._currentCount; } }

        /// <summary>
        /// Exits the <see cref="SemaphoreLight"/> once.
        /// </summary>
        /// <returns>The previous count of the <see cref="SemaphoreLight"/>.</returns>
        public int Release()
        {
            return this.Release(1);
        }

        /// <summary>
        /// Exits the <see cref="SemaphoreLight"/> a specified number of times.
        /// </summary>
        /// <param name="releaseCount">The number of times to exit the semaphore.</param>
        /// <returns>The previous count of the <see cref="SemaphoreLight"/>.</returns>
        public int Release(int releaseCount)
        {
            var oldCount = this._currentCount;

            lock (this._lock)
            {
                this._currentCount += releaseCount;

                Monitor.Pulse(this._lock);
            }

            return oldCount;
        }

        /// <summary>
        /// Blocks the current thread until it can enter the <see cref="SemaphoreLight"/>.
        /// </summary>
        public void Wait()
        {

            lock (this._lock)
            {
                while (this._currentCount < 1)
                {
                    Monitor.Wait(this._lock);
                }

                this._currentCount--;

                Monitor.Pulse(this._lock);
            }
        }
    }
}
