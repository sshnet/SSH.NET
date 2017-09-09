#if !FEATURE_THREAD_COUNTDOWNEVENT

using System;
using System.Threading;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Represents a synchronization primitive that is signaled when its count reaches zero.
    /// </summary>
    internal class CountdownEvent : IDisposable
    {
        private int _count;
        private ManualResetEvent _event;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of <see cref="CountdownEvent"/> class with the specified count.
        /// </summary>
        /// <param name="initialCount">The number of signals initially required to set the <see cref="CountdownEvent"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialCount"/> is less than zero.</exception>
        /// <remarks>
        /// If <paramref name="initialCount"/> is <c>zero</c>, the event is created in a signaled state.
        /// </remarks>
        public CountdownEvent(int initialCount)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount");
            }

            _count = initialCount;

            var initialState = _count == 0;
            _event = new ManualResetEvent(initialState);
        }

        /// <summary>
        /// Gets the number of remaining signals required to set the event.
        /// </summary>
        /// <value>
        /// The number of remaining signals required to set the event.
        /// </value>
        public int CurrentCount
        {
            get { return _count; }
        }

        /// <summary>
        /// Indicates whether the <see cref="CountdownEvent"/>'s current count has reached zero.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current count is zero; otherwise, <c>false</c>.
        /// </value>
        public bool IsSet
        {
            get { return _count == 0; }
        }

        /// <summary>
        /// Gets a <see cref="WaitHandle"/> that is used to wait for the event to be set.
        /// </summary>
        /// <value>
        /// A <see cref="WaitHandle"/> that is used to wait for the event to be set.
        /// </value>
        /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
        public WaitHandle WaitHandle
        {
            get
            {
                EnsureNotDisposed();

                return _event;
            }
        }


        /// <summary>
        /// Registers a signal with the <see cref="CountdownEvent"/>, decrementing the value of <see cref="CurrentCount"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the signal caused the count to reach zero and the event was set; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="InvalidOperationException">The current instance is already set.</exception>
        public bool Signal()
        {
            EnsureNotDisposed();

            if (_count <= 0)
                throw new InvalidOperationException("Invalid attempt made to decrement the event's count below zero.");

            var newCount = Interlocked.Decrement(ref _count);
            if (newCount == 0)
            {
                _event.Set();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Increments the <see cref="CountdownEvent"/>'s current count by one.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="InvalidOperationException">The current instance is already set.</exception>
        /// <exception cref="InvalidOperationException"><see cref="CurrentCount"/> is equal to or greather than <see cref="int.MaxValue"/>.</exception>
        public void AddCount()
        {
            EnsureNotDisposed();

            if (_count == int.MaxValue)
                throw new InvalidOperationException("TODO");

            Interlocked.Increment(ref _count);
        }

        /// <summary>
        /// Blocks the current thread until the <see cref="CountdownEvent"/> is set, using a <see cref="TimeSpan"/>
        /// to measure the timeout.
        /// </summary>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="CountdownEvent"/> was set; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
        public bool Wait(TimeSpan timeout)
        {
            EnsureNotDisposed();

            return _event.WaitOne(timeout);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="CountdownEvent"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="CountdownEvent"/>, and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var theEvent = _event;
                if (theEvent != null)
                {
                    _event = null;
                    theEvent.Dispose();
                }

                _disposed = true;
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}

#endif // FEATURE_THREAD_COUNTDOWNEVENT