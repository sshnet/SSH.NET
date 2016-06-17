using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when file or directory is not found.
    /// </summary>
    public partial class SftpPathNotFoundException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPathNotFoundException"/> class.
        /// </summary>
        public SftpPathNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPathNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SftpPathNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPathNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SftpPathNotFoundException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
