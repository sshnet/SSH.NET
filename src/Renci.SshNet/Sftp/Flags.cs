using System;

namespace Renci.SshNet.Sftp
{
    [Flags]
#pragma warning disable S2344 // Enumeration type names should not have "Flags" or "Enum" suffixes
#pragma warning disable MA0062 // Non-flags enums should not be marked with "FlagsAttribute"
    internal enum Flags
#pragma warning restore MA0062 // Non-flags enums should not be marked with "FlagsAttribute"
#pragma warning restore S2344 // Enumeration type names should not have "Flags" or "Enum" suffixes
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// SSH_FXF_READ.
        /// </summary>
        Read = 0x00000001,

        /// <summary>
        /// SSH_FXF_WRITE.
        /// </summary>
        Write = 0x00000002,

        /// <summary>
        /// SSH_FXF_APPEND.
        /// </summary>
        Append = 0x00000004,

        /// <summary>
        /// SSH_FXF_CREAT.
        /// </summary>
        CreateNewOrOpen = 0x00000008,

        /// <summary>
        /// SSH_FXF_TRUNC.
        /// </summary>
        Truncate = 0x00000010,

        /// <summary>
        /// SSH_FXF_EXCL.
        /// </summary>
        CreateNew = 0x00000028
    }
}
