using System;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides additional information for asynchronous command execution
    /// </summary>
    public class CommandAsyncResult : IAsyncResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAsyncResult"/> class.
        /// </summary>
        internal CommandAsyncResult()
        {
        }

        /// <summary>
        /// Gets or sets the bytes received. If SFTP only file bytes are counted.
        /// </summary>
        /// <value>Total bytes received.</value>
        public int BytesReceived { get; set; }

        /// <summary>
        /// Gets or sets the bytes sent by SFTP.
        /// </summary>
        /// <value>Total bytes sent.</value>
        public int BytesSent { get; set; }

        #region IAsyncResult Members

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <returns>A user-defined object that qualifies or contains information about an asynchronous operation.</returns>
        public object AsyncState { get; internal set; }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.</returns>
        public WaitHandle AsyncWaitHandle { get; internal set; }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <returns>true if the asynchronous operation completed synchronously; otherwise, false.</returns>
        public bool CompletedSynchronously { get; internal set; }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>true if the operation is complete; otherwise, false.</returns>
        public bool IsCompleted { get; internal set; }

        #endregion

        /// <summary>
        /// Gets a value indicating whether <see cref="SshCommand.EndExecute(IAsyncResult)"/> was already called for this
        /// <see cref="CommandAsyncResult"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <see cref="SshCommand.EndExecute(IAsyncResult)"/> was already called for this <see cref="CommandAsyncResult"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        internal bool EndCalled { get; set; }
    }
}
