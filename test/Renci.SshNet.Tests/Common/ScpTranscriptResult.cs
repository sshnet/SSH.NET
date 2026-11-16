using System;
using System.Collections.Generic;

namespace Renci.SshNet.Tests.Common
{
    /// <summary>
    /// The outcome of <see cref="ScpServerTranscript.Run"/>.
    /// </summary>
    public sealed class ScpTranscriptResult
    {
        public ScpTranscriptResult(bool completed, Exception exception, IReadOnlyList<byte[]> sentData, IReadOnlyList<string> execRequests)
        {
            Completed = completed;
            Exception = exception;
            SentData = sentData;
            ExecRequests = execRequests;
        }

        /// <summary>
        /// Gets a value indicating whether the operation finished within the timeout.
        /// </summary>
        public bool Completed { get; }

        /// <summary>
        /// Gets the exception thrown by the operation, if any.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets the data the client sent on the channel, one entry per send.
        /// </summary>
        public IReadOnlyList<byte[]> SentData { get; }

        /// <summary>
        /// Gets the commands the client sent in exec requests.
        /// </summary>
        public IReadOnlyList<string> ExecRequests { get; }
    }
}
