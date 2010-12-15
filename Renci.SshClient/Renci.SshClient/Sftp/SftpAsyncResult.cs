using System;
using System.Threading;

namespace Renci.SshClient.Sftp
{
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

        public object AsyncState { get; private set; }

        public WaitHandle AsyncWaitHandle { get; private set; }

        public bool CompletedSynchronously { get; private set; }

        public bool IsCompleted { get; private set; }

        #endregion

        #region IDisposable Members

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

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
                    }

                }

                // Note disposing has been done.
                this._disposed = true;
            }
        }

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
