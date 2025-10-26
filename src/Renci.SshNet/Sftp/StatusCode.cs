namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Specifies status codes returned by the server in response to SFTP requests.
    /// </summary>
    public enum StatusCode
    {
        /// <summary>
        /// SSH_FX_OK.
        /// </summary>
        /// <remarks>
        /// The operation completed successfully.
        /// </remarks>
        Ok = 0,

        /// <summary>
        /// SSH_FX_EOF.
        /// </summary>
        /// <remarks>
        /// An attempt was made to read past the end of the file,
        /// or no more directory entries were available.
        /// </remarks>
        Eof = 1,

        /// <summary>
        /// SSH_FX_NO_SUCH_FILE.
        /// </summary>
        /// <remarks>
        /// A reference was made to a file that does not exist.
        /// </remarks>
        NoSuchFile = 2,

        /// <summary>
        /// SSH_FX_PERMISSION_DENIED.
        /// </summary>
        /// <remarks>
        /// The user does not have sufficient permissions to perform the operation.
        /// </remarks>
        PermissionDenied = 3,

        /// <summary>
        /// SSH_FX_FAILURE.
        /// </summary>
        /// <remarks>
        /// An error occurred, but no specific error code exists to describe
        /// the failure.
        /// </remarks>
        Failure = 4,

        /// <summary>
        /// SSH_FX_BAD_MESSAGE.
        /// </summary>
        /// <remarks>
        /// A badly formatted packet or SFTP protocol incompatibility was detected.
        /// </remarks>
        BadMessage = 5,

        /// <summary>
        /// SSH_FX_NO_CONNECTION.
        /// </summary>
        /// <remarks>
        /// A pseudo-error which indicates that the client has no
        /// connection to the server (it can only be generated locally
        /// by the client, and MUST NOT be returned by servers).
        /// </remarks>
        NoConnection = 6,

        /// <summary>
        /// SSH_FX_CONNECTION_LOST.
        /// </summary>
        /// <remarks>
        /// A pseudo-error which indicates that the connection to the
        /// server has been lost (it can only be generated locally by
        /// the client, and MUST NOT be returned by servers).
        /// </remarks>
        ConnectionLost = 7,

        /// <summary>
        /// SSH_FX_OP_UNSUPPORTED.
        /// </summary>
        /// <remarks>
        /// The operation could not be completed because the server did not support it.
        /// </remarks>
        OperationUnsupported = 8,
    }
}
