
namespace Renci.SshNet.Sftp
{
    internal enum SftpMessageTypes : byte
    {
        /// <summary>
        /// SSH_FXP_INIT
        /// </summary>
        Init = 1,
        /// <summary>
        /// SSH_FXP_VERSION
        /// </summary>
        Version = 2,
        /// <summary>
        /// SSH_FXP_OPEN
        /// </summary>
        Open = 3,
        /// <summary>
        /// SSH_FXP_CLOSE
        /// </summary>
        Close = 4,
        /// <summary>
        /// SSH_FXP_READ
        /// </summary>
        Read = 5,
        /// <summary>
        /// SSH_FXP_WRITE
        /// </summary>
        Write = 6,
        /// <summary>
        /// SSH_FXP_LSTAT
        /// </summary>
        LStat = 7,
        /// <summary>
        /// SSH_FXP_FSTAT
        /// </summary>
        FStat = 8,
        /// <summary>
        /// SSH_FXP_SETSTAT
        /// </summary>
        SetStat = 9,
        /// <summary>
        /// SSH_FXP_FSETSTAT
        /// </summary>
        FSetStat = 10,
        /// <summary>
        /// SSH_FXP_OPENDIR
        /// </summary>
        OpenDir = 11,
        /// <summary>
        /// SSH_FXP_READDIR
        /// </summary>
        ReadDir = 12,
        /// <summary>
        /// SSH_FXP_REMOVE
        /// </summary>
        Remove = 13,
        /// <summary>
        /// SSH_FXP_MKDIR
        /// </summary>
        MkDir = 14,
        /// <summary>
        /// SSH_FXP_RMDIR
        /// </summary>
        RmDir = 15,
        /// <summary>
        /// SSH_FXP_REALPATH
        /// </summary>
        RealPath = 16,
        /// <summary>
        /// SSH_FXP_STAT
        /// </summary>
        Stat = 17,
        /// <summary>
        /// SSH_FXP_RENAME
        /// </summary>
        Rename = 18,
        /// <summary>
        /// SSH_FXP_READLINK
        /// </summary>
        ReadLink = 19,
        /// <summary>
        /// SSH_FXP_SYMLINK
        /// </summary>
        SymLink = 20,
        /// <summary>
        /// SSH_FXP_LINK
        /// </summary>
        Link = 21,
        /// <summary>
        /// SSH_FXP_BLOCK
        /// </summary>
        Block = 22,
        /// <summary>
        /// SSH_FXP_UNBLOCK
        /// </summary>
        Unblock = 23,

        /// <summary>
        /// SSH_FXP_STATUS
        /// </summary>
        Status = 101,
        /// <summary>
        /// SSH_FXP_HANDLE
        /// </summary>
        Handle = 102,
        /// <summary>
        /// SSH_FXP_DATA
        /// </summary>
        Data = 103,
        /// <summary>
        /// SSH_FXP_NAME
        /// </summary>
        Name = 104,
        /// <summary>
        /// SSH_FXP_ATTRS
        /// </summary>
        Attrs = 105,

        /// <summary>
        /// SSH_FXP_EXTENDED
        /// </summary>
        Extended = 200,
        /// <summary>
        /// SSH_FXP_EXTENDED_REPLY
        /// </summary>
        ExtendedReply = 201

    }
}
