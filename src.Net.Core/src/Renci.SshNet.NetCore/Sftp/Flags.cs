using System;

namespace Renci.SshNet.Sftp
{
    [Flags]
    internal enum Flags
    {
        None = 0x00000000,
        /// <summary>
        /// SSH_FXF_READ
        /// </summary>
        Read = 0x00000001,
        /// <summary>
        /// SSH_FXF_WRITE
        /// </summary>
        Write = 0x00000002,
        /// <summary>
        /// SSH_FXF_APPEND
        /// </summary>
        Append = 0x00000004,
        /// <summary>
        /// SSH_FXF_CREAT
        /// </summary>
        CreateNewOrOpen = 0x00000008,
        /// <summary>
        /// SSH_FXF_TRUNC
        /// </summary>
        Truncate = 0x00000010,
        /// <summary>
        /// SSH_FXF_EXCL
        /// </summary>
        CreateNew = 0x00000028
    }
}
