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

        private readonly Object _asyncState;

        // Field set at construction which do change after operation completes
        private const Int32 _statePending = 0;

        private const Int32 _stateCompletedSynchronously = 1;

        private const Int32 _stateCompletedAsynchronously = 2;

        private Int32 _completedState = _statePending;

        // Field that may or may not get set depending on usage
        private ManualResetEvent _asyncWaitHandle;

        // Fields set when operation completes
        private Exception _exception;

        /// <summary>
        /// Gets or sets a value indicating whether EndInvoke has been called on the current AsyncResult.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if EndInvoke has been called on the current AsyncResult; otherwise, <c>false</c>.
        /// </value>
        public bool EndInvokeCalled { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResult"/> class.
        /// </summary>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        public AsyncResult(AsyncCallback asyncCallback, Object state)
        {
            this._asyncCallback = asyncCallback;
            this._asyncState = state;
        }

        /// <summary>
        /// Marks asynchronous operation as completed.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="completedSynchronously">if set to <c>true</c> [completed synchronously].</param>
        public void SetAsCompleted(Exception exception, Boolean completedSynchronously)
        {
            // Passing null for exception means no error occurred; this is the common case
            this._exception = exception;

            // The m_CompletedState field MUST be set prior calling the callback
            Int32 prevState = Interlocked.Exchange(ref this._completedState,
               completedSynchronously ? _stateCompletedSynchronously : _stateCompletedAsynchronously);
            if (prevState != _statePending)
                throw new InvalidOperationException("You can set a result only once");

            // If the event exists, set it
            if (this._asyncWaitHandle != null)
                this._asyncWaitHandle.Set();

            // If a callback method was set, call it
            if (this._asyncCallback != null)
                this._asyncCallback(this);
        }

        /// <summary>
        /// Waits until the asynchronous operation completes, and then returns. 
        /// </summary>
        public void EndInvoke()
        {
            // This method assumes that only 1 thread calls EndInvoke for this object
            if (!this.IsCompleted)
            {
                // If the operation isn't done, wait for it
                AsyncWaitHandle.WaitOne();
                AsyncWaitHandle.Close();
                this._asyncWaitHandle = null;  // Allow early GC
            }

            this.EndInvokeCalled = true;

            // Operation is done: if an exception occurred, throw it
            if (this._exception != null)
                throw new SshException(this._exception.Message, this._exception);
        }

        #region Implementation of IAsyncResult

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <returns>A user-defined object that qualifies or contains information about an asynchronous operation.</returns>
        public Object AsyncState { get { return this._asyncState; } }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <returns>true if the asynchronous operation completed synchronously; otherwise, false.</returns>
        public Boolean CompletedSynchronously
        {
            get { return this._completedState == _stateCompletedSynchronously; }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.</returns>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this._asyncWaitHandle == null)
                {
                    var done = this.IsCompleted;
                    var mre = new ManualResetEvent(done);
                    if (Interlocked.CompareExchange(ref this._asyncWaitHandle, mre, null) != null)
                    {
                        // Another thread created this object's event; dispose the event we just created
                        mre.Close();
                    }
                    else
                    {
                        if (!done && this.IsCompleted)
                        {
                            // If the operation wasn't done when we created 
                            // the event but now it is done, set the event
                            this._asyncWaitHandle.Set();
                        }
                    }
                }
                return this._asyncWaitHandle;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>true if the operation is complete; otherwise, false.</returns>
        public Boolean IsCompleted
        {
            get { return this._completedState != _statePending; }
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
        private TResult _result = default(TResult);

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResult&lt;TResult&gt;"/> class.
        /// </summary>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        public AsyncResult(AsyncCallback asyncCallback, Object state)
            : base(asyncCallback, state)
        {
        }

        /// <summary>
        /// Marks asynchronous operation as completed.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="completedSynchronously">if set to <c>true</c> [completed synchronously].</param>
        public void SetAsCompleted(TResult result, Boolean completedSynchronously)
        {
            // Save the asynchronous operation's result
            this._result = result;

            // Tell the base class that the operation completed successfully (no exception)
            base.SetAsCompleted(null, completedSynchronously);
        }

        /// <summary>
        /// Waits until the asynchronous operation completes, and then returns the value generated by the asynchronous operation. 
        /// </summary>
        /// <returns>Invocation result</returns>
        new public TResult EndInvoke()
        {
            base.EndInvoke(); // Wait until operation has completed 
            return _result;  // Return the result (if above didn't throw)
        }
    }
}