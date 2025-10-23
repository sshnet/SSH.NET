#nullable enable
using System;

using Renci.SshNet.Sftp;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when operation permission is denied.
    /// </summary>
#if NETFRAMEWORK
    [Serializable]
#endif
    public class SftpPermissionDeniedException : SftpException
    {
        private const StatusCode Code = StatusCode.PermissionDenied;

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPermissionDeniedException"/> class.
        /// </summary>
        public SftpPermissionDeniedException()
            : base(Code)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPermissionDeniedException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string)" path="/param"/>
        public SftpPermissionDeniedException(string? message)
            : base(Code, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpPermissionDeniedException"/> class.
        /// </summary>
        /// <inheritdoc cref="Exception(string, Exception)" path="/param"/>
        public SftpPermissionDeniedException(string? message, Exception? innerException)
            : base(Code, message, innerException)
        {
        }

#if NETFRAMEWORK
        /// <inheritdoc/>
        protected SftpPermissionDeniedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
