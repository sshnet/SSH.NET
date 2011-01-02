using System;
using System.Threading;

namespace Renci.SshClient.Sftp
{
    /// <summary>
    /// Represents the status of an asynchronous SFTP operation.
    /// </summary>
    public class SftpAsyncResult : IAsyncResult, IDisposable
    {
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

        private SftpCommand _command;

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpAsyncResult"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="state">The state.</param>
        internal SftpAsyncResult(SftpCommand command, object state)
        {
            this._command = command;
            this.AsyncState = state;
            this.AsyncWaitHandle = new ManualResetEvent(false);
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <typeparam name="T">Type of the command</typeparam>
        /// <returns></returns>
        internal T GetCommand<T>() where T : SftpCommand
        {
            T cmd = this._command as T;

            if (cmd == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            return cmd;
        }

        #region IAsyncResult Members

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <returns>A user-defined object that qualifies or contains information about an asynchronous operation.</returns>
        public object AsyncState { get; private set; }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.</returns>
        public WaitHandle AsyncWaitHandle { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <returns>true if the asynchronous operation completed synchronously; otherwise, false.</returns>
        public bool CompletedSynchronously { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>true if the operation is complete; otherwise, false.</returns>
        public bool IsCompleted { get; private set; }

        #endregion

        #region IDisposable Members

        private bool _disposed = false;

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
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this.AsyncWaitHandle != null)
                    {
                        this.AsyncWaitHandle.Dispose();
                        this.AsyncWaitHandle = null;
                    }

                }

                // Note disposing has been done.
                this._disposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SftpAsyncResult"/> is reclaimed by garbage collection.
        /// </summary>
        ~SftpAsyncResult()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

        /// <summary>
        /// Completes asynchronous operation.
        /// </summary>
        internal void Complete()
        {
            this.IsCompleted = true;
            ((EventWaitHandle)this.AsyncWaitHandle).Set();
        }
    }
}
