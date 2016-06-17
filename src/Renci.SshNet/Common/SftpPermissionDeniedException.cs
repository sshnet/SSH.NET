using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when operation permission is denied.
    /// </summary>
    public partial class SftpPermissionDeniedException : SshException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPermissionDeniedException"/> class.
        /// </summary>
        public SftpPermissionDeniedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPermissionDeniedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SftpPermissionDeniedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPermissionDeniedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SftpPermissionDeniedException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
