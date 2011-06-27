using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Encapsulates the results of an asynchronous upload operation.
    /// </summary>
    public class SftpUploadAsyncResult : AsyncResult
    {
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
            this.UploadedBytes = uploadedBytes;
        }

    }
}
