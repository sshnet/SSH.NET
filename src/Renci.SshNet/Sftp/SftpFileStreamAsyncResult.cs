using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Encapsulates the results of an asynchronous read or write operation.
    /// </summary>
    public class SftpFileStreamAsyncResult :  AsyncResult
    {
        /// <summary>
        /// Gets the number of read or written bytes.
        /// </summary>
        public int Bytes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpFileStreamAsyncResult"/> class.
        /// </summary>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        public SftpFileStreamAsyncResult(AsyncCallback asyncCallback, object state)
            : base(asyncCallback, state)
        {
        }

        /// <summary>
        /// Updates asynchronous operation status information.
        /// </summary>
        /// <param name="bytes">Number of IO bytes in this operation.</param>
        internal void Update(int bytes)
        {
            Bytes = bytes;
        }
    }
}
