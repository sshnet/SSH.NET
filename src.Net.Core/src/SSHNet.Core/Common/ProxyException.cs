using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when SCP error occurred.
    /// </summary>
    public partial class ProxyException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        public ProxyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ProxyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ProxyException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }

}
