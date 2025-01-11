using System;
using System.Collections.Generic;
using System.IO;

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Encapsulates the results of an asynchronous directory synchronization operation.
    /// </summary>
    public class SftpSynchronizeDirectoriesAsyncResult : AsyncResult<IEnumerable<FileInfo>>
    {
        /// <summary>
        /// Gets the number of files read so far.
        /// </summary>
        public int FilesRead { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpSynchronizeDirectoriesAsyncResult" /> class.
        /// </summary>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        public SftpSynchronizeDirectoriesAsyncResult(AsyncCallback asyncCallback, object state)
            : base(asyncCallback, state)
        {
        }

        /// <summary>
        /// Updates asynchronous operation status information.
        /// </summary>
        /// <param name="filesRead">The files read.</param>
        internal void Update(int filesRead)
        {
            FilesRead = filesRead;
        }
    }
}
