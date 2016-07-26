using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Encapsulates the results of an asynchronous download operation.
    /// </summary>
    public class SftpDownloadAsyncResult :  AsyncResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel asynchronous download operation.
        /// </summary>
        /// <value>
        /// <c>true</c> if download operation to be canceled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Download operation will be canceled after finishing uploading current buffer.
        /// </remarks>
        public bool IsDownloadCanceled { get; set; }

        /// <summary>
        /// Gets the number of downloaded bytes.
        /// </summary>
        public ulong DownloadedBytes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpDownloadAsyncResult"/> class.
        /// </summary>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        public SftpDownloadAsyncResult(AsyncCallback asyncCallback, object state)
            : base(asyncCallback, state)
        {
        }

        /// <summary>
        /// Updates asynchronous operation status information.
        /// </summary>
        /// <param name="downloadedBytes">Number of downloaded bytes.</param>
        internal void Update(ulong downloadedBytes)
        {
            DownloadedBytes = downloadedBytes;
        }
    }
}
