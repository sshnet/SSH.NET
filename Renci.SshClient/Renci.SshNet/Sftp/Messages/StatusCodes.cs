
namespace Renci.SshNet.Sftp.Messages
{
    internal enum StatusCodes : uint
    {
        /// <summary>
        /// SSH_FX_OK
        /// </summary>
        Ok = 0,
        /// <summary>
        /// SSH_FX_EOF
        /// </summary>
        Eof = 1,
        /// <summary>
        /// SSH_FX_NO_SUCH_FILE
        /// </summary>
        NoSuchFile = 2,
        /// <summary>
        /// SSH_FX_PERMISSION_DENIED
        /// </summary>
        PermissionDenied = 3,
        /// <summary>
        /// SSH_FX_FAILURE
        /// </summary>
        Failure = 4,
        /// <summary>
        /// SSH_FX_BAD_MESSAGE
        /// </summary>
        BadMessage = 5,
        /// <summary>
        /// SSH_FX_NO_CONNECTION
        /// </summary>
        NoConnection = 6,
        /// <summary>
        /// SSH_FX_CONNECTION_LOST
        /// </summary>
        ConnectionLost = 7,
        /// <summary>
        /// SSH_FX_OP_UNSUPPORTED
        /// </summary>
        OperationUnsupported = 8,
    }
}
