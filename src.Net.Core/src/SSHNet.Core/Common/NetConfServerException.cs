using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when there is something wrong with the server capabilities.
    /// </summary>
    public partial class NetConfServerException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfServerException"/> class.
        /// </summary>
        public NetConfServerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfServerException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public NetConfServerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetConfServerException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NetConfServerException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
