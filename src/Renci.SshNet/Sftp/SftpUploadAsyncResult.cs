using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Encapsulates the results of an asynchronous upload operation.
    /// </summary>
    public class SftpUploadAsyncResult : AsyncResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel asynchronous upload operation
        /// </summary>
        /// <value>
        /// <c>true</c> if upload operation to be canceled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Upload operation will be canceled after finishing uploading current buffer.
        /// </remarks>
        public bool IsUploadCanceled { get; set; }

        /// <summary>
        /// Gets the number of uploaded bytes.
        /// </summary>
        public ulong UploadedBytes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpUploadAsyncResult"/> class.
        /// </summary>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        public SftpUploadAsyncResult(AsyncCallback asyncCallback, Object state)
            : base(asyncCallback, state)
        {
        }

        /// <summary>
        /// Updates asynchronous operation status information.
        /// </summary>
        /// <param name="uploadedBytes">Number of uploaded bytes.</param>
        internal void Update(ulong uploadedBytes)
        {
            UploadedBytes = uploadedBytes;
        }
    }
}
