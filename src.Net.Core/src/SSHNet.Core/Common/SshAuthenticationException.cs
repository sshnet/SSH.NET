using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when authentication failed.
    /// </summary>
    public partial class SshAuthenticationException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshAuthenticationException"/> class.
        /// </summary>
        public SshAuthenticationException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshAuthenticationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshAuthenticationException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshAuthenticationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SshAuthenticationException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
