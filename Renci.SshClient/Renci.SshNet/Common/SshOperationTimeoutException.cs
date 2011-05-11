using System;

namespace Renci.SshClient.Common
{
    /// <summary>
    /// The exception that is thrown when operation is timed out.
    /// </summary>
    [Serializable]
    public class SshOperationTimeoutException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshOperationTimeoutException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshOperationTimeoutException(string message)
            : base(message)
        {

        }
    }
}
