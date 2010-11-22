using System;
using System.Threading;

namespace Renci.SshClient
{
    public class CommandAsyncResult : IAsyncResult
    {
        /// <summary>
        /// Gets or sets the command that async result was created for.
        /// </summary>
        /// <value>The channel.</value>
        internal SshCommand Command { get; private set; }

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

        public object AsyncState { get; internal set; }

        public WaitHandle AsyncWaitHandle { get; internal set; }

        public bool CompletedSynchronously { get; internal set; }

        public bool IsCompleted { get; internal set; }

        #endregion

        internal CommandAsyncResult(SshCommand command)
        {
            this.Command = command;
        }
    }
}
