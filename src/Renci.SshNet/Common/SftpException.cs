#nullable enable
using System;

using Renci.SshNet.Sftp;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// The exception that is thrown when an error occurs in the SFTP layer.
    /// </summary>
#if NETFRAMEWORK
    [Serializable]
#endif
#pragma warning disable CA1032 // Implement standard exception constructors
    public class SftpException : SshException
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        /// <summary>
        /// Gets the status code that is associated with this exception.
        /// </summary>
        public StatusCode StatusCode { get; }

        /// <summary>
        /// Gets the path that is associated with this exception.
        /// </summary>
        /// <value>
        /// The path that the failed operation was performed on, or <see langword="null"/>
        /// if no path was passed to the constructor for this instance.
        /// </value>
        /// <remarks>
        /// This is populated only for operations which act on a single path, such as
        /// opening or deleting a file. It is <see langword="null"/> for operations acting
        /// on an open handle or on more than one path, such as renaming a file.
        /// </remarks>
        public string? Path { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpException"/> class.
        /// </summary>
        /// <param name="statusCode">The status code that indicates the error that occurred.</param>
        public SftpException(StatusCode statusCode)
            : this(statusCode, message: null, innerException: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpException"/> class.
        /// </summary>
        /// <param name="statusCode">The status code that indicates the error that occurred.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public SftpException(StatusCode statusCode, string? message)
            : this(statusCode, message, innerException: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpException"/> class.
        /// </summary>
        /// <param name="statusCode">The status code that indicates the error that occurred.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SftpException(StatusCode statusCode, string? message, Exception? innerException)
            : this(statusCode, message, path: null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpException"/> class.
        /// </summary>
        /// <param name="statusCode">The status code that indicates the error that occurred.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="path">The path that the failed operation was performed on.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SftpException(StatusCode statusCode, string? message, string? path, Exception? innerException)
            : base(string.IsNullOrEmpty(message) ? GetDefaultMessage(statusCode, path) : message, innerException)
        {
            StatusCode = statusCode;
            Path = path;
        }

        private static string GetDefaultMessage(StatusCode statusCode, string? path)
        {
#pragma warning disable IDE0072 // Add missing cases
            var message = statusCode switch
            {
                StatusCode.Ok => "The operation completed successfully.",
                StatusCode.NoSuchFile => "A reference was made to a file that does not exist.",
                StatusCode.PermissionDenied => "The user does not have sufficient permissions to perform the operation.",
                StatusCode.Failure => "An error occurred, but no specific error code exists to describe the failure.",
                StatusCode.BadMessage => "A badly formatted packet or SFTP protocol incompatibility was detected.",
                StatusCode.OperationUnsupported => "An attempt was made to perform an operation which is not supported.",
                _ => statusCode.ToString()
            };
#pragma warning restore IDE0072 // Add missing cases

            return path is not null
                ? $"{message} Path: '{path}'."
                : message;
        }

#if NETFRAMEWORK
        /// <inheritdoc/>
        protected SftpException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
