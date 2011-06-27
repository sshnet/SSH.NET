using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Encapsulates the results of an asynchronous download operation.
    /// </summary>
    public class SftpDownloadAsyncResult :  AsyncResult
    {
        /// <summary>
        /// Gets the number of downloaded bytes.
        /// </summary>
        public ulong DownloadedBytes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpDownloadAsyncResult"/> class.
        /// </summary>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        public SftpDownloadAsyncResult(AsyncCallback asyncCallback, Object state)
            : base(asyncCallback, state)
        {

        }

        /// <summary>
        /// Updates asynchronous operation status information.
        /// </summary>
        /// <param name="downloadedBytes">Number of downloaded bytes.</param>
        internal void Update(ulong downloadedBytes)
        {
            this.DownloadedBytes = downloadedBytes;
        }
    }
}
