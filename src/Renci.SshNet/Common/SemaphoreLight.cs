using System;
using System.Threading;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Light implementation of SemaphoreSlim.
    /// </summary>
    public class SemaphoreLight : IDisposable
    {
        private readonly object _lock = new object();
        private ManualResetEvent _waitHandle;

        private int _currentCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphoreLight"/> class, specifying 
        /// the initial number of requests that can be granted concurrently.
        /// </summary>
        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialCount"/> is a negative number.</exception>
        public SemaphoreLight(int initialCount)
        {
            if (initialCount < 0 )
                throw new ArgumentOutOfRangeException("initialCount", "The value cannot be negative.");

            _currentCount = initialCount;
        }

        /// <summary>
        /// Gets the current count of the <see cref="SemaphoreLight"/>.
        /// </summary>
        public int CurrentCount { get { return _currentCount; } }

        /// <summary>
        /// Returns a <see cref="WaitHandle"/> that can be used to wait on the semaphore.
        /// </summary>
        /// <value>
        /// A <see cref="WaitHandle"/> that can be used to wait on the semaphore.
        /// </value>
        /// <remarks>
        /// A successful wait on the <see cref="AvailableWaitHandle"/> does not imply a successful
        /// wait on the <see cref="SemaphoreLight"/> itself. It should be followed by a true wait
        /// on the semaphore.
        /// </remarks>
        public WaitHandle AvailableWaitHandle
        {
            get
            {
                if (_waitHandle == null)
                {
                    lock (_lock)
                    {
                        if (_waitHandle == null)
                        {
                            _waitHandle = new ManualResetEvent(_currentCount > 0);
                        }
                    }
                }

                return _waitHandle;
            }
        }

        /// <summary>
        /// Exits the <see cref="SemaphoreLight"/> once.
        /// </summary>
        /// <returns>The previous count of the <see cref="SemaphoreLight"/>.</returns>
        public int Release()
        {
            return Release(1);
        }

        /// <summary>
        /// Exits the <see cref="SemaphoreLight"/> a specified number of times.
        /// </summary>
        /// <param name="releaseCount">The number of times to exit the semaphore.</param>
        /// <returns>
        /// The previous count of the <see cref="SemaphoreLight"/>.
        /// </returns>
        public int Release(int releaseCount)
        {
            lock (_lock)
            {
                var oldCount = _currentCount;

                _currentCount += releaseCount;

                // signal waithandle when the original semaphore count was zero
                if (_waitHandle != null && oldCount == 0)
                {
                    _waitHandle.Set();
                }

                Monitor.PulseAll(_lock);

                return oldCount;
            }
        }

        /// <summary>
        /// Blocks the current thread until it can enter the <see cref="SemaphoreLight"/>.
        /// </summary>
        public void Wait()
        {
            lock (_lock)
            {
                while (_currentCount < 1)
                {
                    Monitor.Wait(_lock);
                }

                _currentCount--;

                // unsignal waithandle when the semaphore count reaches zero
                if (_waitHandle != null && _currentCount == 0)
                {
                    _waitHandle.Reset();
                }

                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Blocks the current thread until it can enter the <see cref="SemaphoreLight"/>, using a 32-bit signed
        /// integer that specifies the timeout.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Infinite(-1) to wait indefinitely.</param>
        /// <returns>
        /// <c>true</c> if the current thread successfully entered the <see cref="SemaphoreLight"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Wait(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeout", "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");

            return WaitWithTimeout(millisecondsTimeout);
        }

        /// <summary>
        /// Blocks the current thread until it can enter the <see cref="SemaphoreLight"/>, using a <see cref="TimeSpan"/>
        /// to specify the timeout.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
        /// <returns>
        /// <c>true</c> if the current thread successfully entered the <see cref="SemaphoreLight"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Wait(TimeSpan timeout)
        {
            var timeoutInMilliseconds = timeout.TotalMilliseconds;
            if (timeoutInMilliseconds < -1d || timeoutInMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException("timeout", "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");

            return WaitWithTimeout((int) timeoutInMilliseconds);
        }

        private bool WaitWithTimeout(int timeoutInMilliseconds)
        {
            lock (_lock)
            {
                if (timeoutInMilliseconds == Session.Infinite)
                {
                    while (_currentCount < 1)
                        Monitor.Wait(_lock);
                }
                else
                {
                    if (_currentCount < 1)
                    {
                        if (timeoutInMilliseconds > 0)
                            return false;

                        var remainingTimeInMilliseconds = timeoutInMilliseconds;
                        var startTicks = Environment.TickCount;

                        while (_currentCount < 1)
                        {
                            if (!Monitor.Wait(_lock, remainingTimeInMilliseconds))
                            {
                                return false;
                            }

                            var elapsed = Environment.TickCount - startTicks;
                            remainingTimeInMilliseconds -= elapsed;
                            if (remainingTimeInMilliseconds < 0)
                                return false;
                        }
                    }
                }

                _currentCount--;

                // unsignal waithandle when the semaphore count is zero
                if (_waitHandle != null && _currentCount == 0)
                {
                    _waitHandle.Reset();
                }

                Monitor.PulseAll(_lock);

                return true;
            }
        }

        /// <summary>
        /// Finalizes the current <see cref="SemaphoreLight"/>.
        /// </summary>
        ~SemaphoreLight()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                var waitHandle = _waitHandle;
                if (waitHandle != null)
                {
                    waitHandle.Dispose();
                    _waitHandle = null;
                }
            }
        }
    }
}
