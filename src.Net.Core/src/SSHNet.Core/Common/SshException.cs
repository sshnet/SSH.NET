using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when SSH exception occurs.
    /// </summary>
    public partial class SshException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshException"/> class.
        /// </summary>
        public SshException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SshException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public SshException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
