using System;
using System.Threading;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base class to encapsulates the results of an asynchronous operation.
    /// </summary>
    public abstract class AsyncResult : IAsyncResult
    {
        // Fields set at construction which never change while operation is pending
        private readonly AsyncCallback _asyncCallback;

        private readonly object _asyncState;

        // Field set at construction which do change after operation completes
        private const int StatePending = 0;

        private const int StateCompletedSynchronously = 1;

        private const int StateCompletedAsynchronously = 2;

        private int _completedState = StatePending;

        // Field that may or may not get set depending on usage
        private ManualResetEvent _asyncWaitHandle;

        // Fields set when operation completes
        private Exception _exception;

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="EndInvoke()"/> has been called on the current
        /// <see cref="AsyncResult"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if <see cref="EndInvoke()"/> has been called on the current <see cref="AsyncResult"/>;
        /// otherwise, <c>false</c>.
        /// </value>
        public bool EndInvokeCalled { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResult"/> class.
        /// </summary>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        protected AsyncResult(AsyncCallback asyncCallback, object state)
        {
            _asyncCallback = asyncCallback;
            _asyncState = state;
        }

        /// <summary>
        /// Marks asynchronous operation as completed.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="completedSynchronously">if set to <c>true</c> [completed synchronously].</param>
        public void SetAsCompleted(Exception exception, bool completedSynchronously)
        {
            // Passing null for exception means no error occurred; this is the common case
            _exception = exception;

            // The m_CompletedState field MUST be set prior calling the callback
            var prevState = Interlocked.Exchange(ref _completedState,
               completedSynchronously ? StateCompletedSynchronously : StateCompletedAsynchronously);
            if (prevState != StatePending)
                throw new InvalidOperationException("You can set a result only once");

            // If the event exists, set it
            if (_asyncWaitHandle != null)
            {
                _asyncWaitHandle.Set();
            }

            // If a callback method was set, call it
            if (_asyncCallback != null)
            {
                _asyncCallback(this);
            }
        }

        /// <summary>
        /// Waits until the asynchronous operation completes, and then returns. 
        /// </summary>
        internal void EndInvoke()
        {
            // This method assumes that only 1 thread calls EndInvoke for this object
            if (!IsCompleted)
            {
                // If the operation isn't done, wait for it
                AsyncWaitHandle.WaitOne();
                _asyncWaitHandle = null;  // Allow early GC
                AsyncWaitHandle.Dispose();
            }

            EndInvokeCalled = true;

            // Operation is done: if an exception occurred, throw it
            if (_exception != null)
                throw _exception;
        }

        #region Implementation of IAsyncResult

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <returns>A user-defined object that qualifies or contains information about an asynchronous operation.</returns>
        public object AsyncState { get { return _asyncState; } }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <returns>true if the asynchronous operation completed synchronously; otherwise, false.</returns>
        public bool CompletedSynchronously
        {
            get { return _completedState == StateCompletedSynchronously; }
        }

        /// <summary>
        /// Gets a <see cref="WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>A <see cref="WaitHandle"/> that is used to wait for an asynchronous operation to complete.</returns>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_asyncWaitHandle == null)
                {
                    var done = IsCompleted;
                    var mre = new ManualResetEvent(done);
                    if (Interlocked.CompareExchange(ref _asyncWaitHandle, mre, null) != null)
                    {
                        // Another thread created this object's event; dispose the event we just created
                        mre.Dispose();
                    }
                    else
                    {
                        if (!done && IsCompleted)
                        {
                            // If the operation wasn't done when we created 
                            // the event but now it is done, set the event
                            _asyncWaitHandle.Set();
                        }
                    }
                }
                return _asyncWaitHandle;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the operation is complete; otherwise, <c>false</c>.</returns>
        public bool IsCompleted
        {
            get { return _completedState != StatePending; }
        }

        #endregion
    }

    /// <summary>
    /// Base class to encapsulates the results of an asynchronous operation that returns result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class AsyncResult<TResult> : AsyncResult
    {
        // Field set when operation completes
        private TResult _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResult{TResult}"/> class.
        /// </summary>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        protected AsyncResult(AsyncCallback asyncCallback, object state)
            : base(asyncCallback, state)
        {
        }

        /// <summary>
        /// Marks asynchronous operation as completed.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="completedSynchronously">if set to <c>true</c> [completed synchronously].</param>
        public void SetAsCompleted(TResult result, bool completedSynchronously)
        {
            // Save the asynchronous operation's result
            _result = result;

            // Tell the base class that the operation completed successfully (no exception)
            SetAsCompleted(null, completedSynchronously);
        }

        /// <summary>
        /// Waits until the asynchronous operation completes, and then returns the value generated by the asynchronous operation. 
        /// </summary>
        /// <returns>
        /// The invocation result.
        /// </returns>
        public new TResult EndInvoke()
        {
            base.EndInvoke(); // Wait until operation has completed 
            return _result;  // Return the result (if above didn't throw)
        }
    }
}