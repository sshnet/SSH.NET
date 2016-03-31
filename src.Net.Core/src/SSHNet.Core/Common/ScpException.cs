using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when SCP error occurred.
    /// </summary>
    public partial class ScpException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        public ScpException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ScpException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ScpException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
