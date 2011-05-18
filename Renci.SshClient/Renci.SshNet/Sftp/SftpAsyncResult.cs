using System;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Represents the status of an asynchronous SFTP operation.
    /// </summary>
    public class SftpAsyncResult : IAsyncResult
    {
        private const int _statePending = 0;

        private const int _stateCompletedSynchronously = 1;

        private const int _stateCompletedAsynchronously = 2;

        private readonly AsyncCallback _asyncCallback;

        private readonly Object _asyncState;

        private Exception _exception;

        private ManualResetEvent _asyncWaitHandle;

        private int _completedState = _statePending;

        private SftpSession _sftpSession;

        private TimeSpan _commandTimeout;

        /// <summary>
        /// Gets or sets the uploaded bytes.
        /// </summary>
        /// <value>The uploaded bytes.</value>
        public ulong UploadedBytes { get; internal set; }

        /// <summary>
        /// Gets or sets the downloaded bytes.
        /// </summary>
        /// <value>The downloaded bytes.</value>
        public ulong DownloadedBytes { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpAsyncResult"/> class.
        /// </summary>
        /// <param name="sftpSession">The SFTP session.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        internal SftpAsyncResult(SftpSession sftpSession, TimeSpan commandTimeout, AsyncCallback asyncCallback, object state)
        {
            this._sftpSession = sftpSession;
            this._commandTimeout = commandTimeout;
            this._asyncCallback = asyncCallback;
            this._asyncState = state;
        }

        /// <summary>
        /// Marks result as completed.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="completedSynchronously">if set to <c>true</c> [completed synchronously].</param>
        public void SetAsCompleted(Exception exception, bool completedSynchronously)
        {
            // Passing null for exception means no error occurred. 
            // This is the common case
            this._exception = exception;

            // The _completedState field MUST be set prior calling the callback
            Int32 prevState = Interlocked.Exchange(ref this._completedState, completedSynchronously ? _stateCompletedSynchronously : _stateCompletedAsynchronously);
            if (prevState != _statePending)
                throw new InvalidOperationException("You can set a result only once");

            // If the event exists, set it
            if (this._asyncWaitHandle != null)
                this._asyncWaitHandle.Set();

            // If a callback method was set, call it on different thread
            if (this._asyncCallback != null)
                Task.Factory.StartNew(() => { this._asyncCallback(this); });
        }

        public void EndInvoke()
        {
            // This method assumes that only 1 thread calls EndInvoke 
            // for this object
            if (!this.IsCompleted)
            {
                // If the operation isn't done, wait for it
                this._sftpSession.WaitHandle(this.AsyncWaitHandle, this._commandTimeout);
                this.AsyncWaitHandle.Close();
                this._asyncWaitHandle = null;  // Allow early GC
            }

            // Operation is done: if an exception occurred, throw it
            if (this._exception != null)
                throw _exception;
        }

        #region IAsyncResult Members

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
            get
            {
                return Thread.VolatileRead(ref this._completedState) == _stateCompletedSynchronously;
            }
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
                    Boolean done = this.IsCompleted;
                    ManualResetEvent mre = new ManualResetEvent(done);
                    if (Interlocked.CompareExchange(ref this._asyncWaitHandle, mre, null) != null)
                    {
                        // Another thread created this object's event; dispose 
                        // the event we just created
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
            get
            {
                return Thread.VolatileRead(ref this._completedState) != _statePending;
            }
        }

        #endregion
    }
}
