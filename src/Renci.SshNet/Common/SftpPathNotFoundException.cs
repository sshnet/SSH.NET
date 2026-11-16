#nullable enable
using System;

using Renci.SshNet.Sftp;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when file or directory is not found.
    /// </summary>
#if NETFRAMEWORK
    [Serializable]
#endif
    public class SftpPathNotFoundException : SftpException
    {
        private const StatusCode Code = StatusCode.NoSuchFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPathNotFoundException"/> class.
        /// </summary>
        public SftpPathNotFoundException()
            : this(message: null, path: null, innerException: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPathNotFoundException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string)" path="/param"/>
        public SftpPathNotFoundException(string? message)
            : this(message, path: null, innerException: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPathNotFoundException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string)" path="/param"/>
        public SftpPathNotFoundException(string? message, string? path)
            : this(message, path, innerException: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPathNotFoundException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
        public SftpPathNotFoundException(string? message, Exception? innerException)
            : this(message, path: null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPathNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="path">The path that cannot be found.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SftpPathNotFoundException(string? message, string? path, Exception? innerException)
            : base(Code, message, path, innerException)
        {
        }

#if NETFRAMEWORK
        /// <inheritdoc/>
        protected SftpPathNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
