
namespace Renci.SshNet.Sftp
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
        /// <summary>
        /// SSH_FX_INVALID_HANDLE
        /// </summary>
        InvalidHandle = 9,
        /// <summary>
        /// SSH_FX_NO_SUCH_PATH
        /// </summary>
        NoSuchPath = 10,
        /// <summary>
        /// SSH_FX_FILE_ALREADY_EXISTS
        /// </summary>
        FileAlreadyExists = 11,
        /// <summary>
        /// SSH_FX_WRITE_PROTECT
        /// </summary>
        WriteProtect = 12,
        /// <summary>
        /// SSH_FX_NO_MEDIA
        /// </summary>
        NoMedia = 13,
        /// <summary>
        /// SSH_FX_NO_SPACE_ON_FILESYSTEM
        /// </summary>
        NoSpaceOnFilesystem = 14,
        /// <summary>
        /// SSH_FX_QUOTA_EXCEEDED
        /// </summary>
        QuotaExceeded = 15,
        /// <summary>
        /// SSH_FX_UNKNOWN_PRINCIPAL
        /// </summary>
        UnknownPrincipal = 16,
        /// <summary>
        /// SSH_FX_LOCK_CONFLICT
        /// </summary>
        LockConflict = 17,
        /// <summary>
        /// SSH_FX_DIR_NOT_EMPTY
        /// </summary>
        DirNotEmpty = 18,
        /// <summary>
        /// SSH_FX_NOT_A_DIRECTORY
        /// </summary>
        NotDirectory = 19,
        /// <summary>
        /// SSH_FX_INVALID_FILENAME
        /// </summary>
        InvalidFilename = 20,
        /// <summary>
        /// SSH_FX_LINK_LOOP
        /// </summary>
        LinkLoop = 21,
        /// <summary>
        /// SSH_FX_CANNOT_DELETE
        /// </summary>
        CannotDelete = 22,
        /// <summary>
        /// SSH_FX_INVALID_PARAMETER
        /// </summary>
        InvalidParameter = 23,
        /// <summary>
        /// SSH_FX_FILE_IS_A_DIRECTORY
        /// </summary>
        FileIsADirectory = 24,
        /// <summary>
        /// SSH_FX_BYTE_RANGE_LOCK_CONFLICT
        /// </summary>
        ByteRangeLockConflict = 25,
        /// <summary>
        /// SSH_FX_BYTE_RANGE_LOCK_REFUSED
        /// </summary>
        ByteRangeLockRefused = 26,
        /// <summary>
        /// SSH_FX_DELETE_PENDING
        /// </summary>
        DeletePending = 27,
        /// <summary>
        /// SSH_FX_FILE_CORRUPT
        /// </summary>
        FileCorrupt = 28,
        /// <summary>
        /// SSH_FX_OWNER_INVALID
        /// </summary>
        OwnerInvalid = 29,
        /// <summary>
        /// SSH_FX_GROUP_INVALID
        /// </summary>
        GroupInvalid = 30,
        /// <summary>
        /// SSH_FX_NO_MATCHING_BYTE_RANGE_LOCK
        /// </summary>
        NoMatchingByteRangeLock = 31,
    }
}
