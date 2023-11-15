﻿using System;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Represents SFTP file information.
    /// </summary>
    public interface ISftpFile
    {
        /// <summary>
        /// Gets the file attributes.
        /// </summary>
        SftpFileAttributes Attributes { get; }

        /// <summary>
        /// Gets the full path of the file or directory.
        /// </summary>
        /// <value>
        /// The full path of the file or directory.
        /// </value>
        string FullName { get; }

        /// <summary>
        /// Gets the name of the file or directory.
        /// </summary>
        /// <value>
        /// The name of the file or directory.
        /// </value>
        /// <remarks>
        /// For directories, this is the name of the last directory in the hierarchy if a hierarchy exists;
        /// otherwise, the name of the directory.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets or sets the time the current file or directory was last accessed.
        /// </summary>
        /// <value>
        /// The time that the current file or directory was last accessed.
        /// </value>
        DateTime LastAccessTime { get; set; }

        /// <summary>
        /// Gets or sets the time when the current file or directory was last written to.
        /// </summary>
        /// <value>
        /// The time the current file was last written.
        /// </value>
        DateTime LastWriteTime { get; set; }

        /// <summary>
        /// Gets or sets the time, in coordinated universal time (UTC), the current file or directory was last accessed.
        /// </summary>
        /// <value>
        /// The time that the current file or directory was last accessed.
        /// </value>
        DateTime LastAccessTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the time, in coordinated universal time (UTC), when the current file or directory was last written to.
        /// </summary>
        /// <value>
        /// The time the current file was last written.
        /// </value>
        DateTime LastWriteTimeUtc { get; set; }

        /// <summary>
        /// Gets the size, in bytes, of the current file.
        /// </summary>
        /// <value>
        /// The size of the current file in bytes.
        /// </value>
        long Length { get; }

        /// <summary>
        /// Gets or sets file user id.
        /// </summary>
        /// <value>
        /// File user id.
        /// </value>
        int UserId { get; set; }

        /// <summary>
        /// Gets or sets file group id.
        /// </summary>
        /// <value>
        /// File group id.
        /// </value>
        int GroupId { get; set; }

        /// <summary>
        /// Gets a value indicating whether file represents a socket.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a socket; otherwise, <see langword="false"/>.
        /// </value>
        bool IsSocket { get; }

        /// <summary>
        /// Gets a value indicating whether file represents a symbolic link.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a symbolic link; otherwise, <see langword="false"/>.
        /// </value>
        bool IsSymbolicLink { get; }

        /// <summary>
        /// Gets a value indicating whether file represents a regular file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a regular file; otherwise, <see langword="false"/>.
        /// </value>
        bool IsRegularFile { get; }

        /// <summary>
        /// Gets a value indicating whether file represents a block device.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a block device; otherwise, <see langword="false"/>.
        /// </value>
        bool IsBlockDevice { get; }

        /// <summary>
        /// Gets a value indicating whether file represents a directory.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a directory; otherwise, <see langword="false"/>.
        /// </value>
        bool IsDirectory { get; }

        /// <summary>
        /// Gets a value indicating whether file represents a character device.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a character device; otherwise, <see langword="false"/>.
        /// </value>
        bool IsCharacterDevice { get; }

        /// <summary>
        /// Gets a value indicating whether file represents a named pipe.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a named pipe; otherwise, <see langword="false"/>.
        /// </value>
        bool IsNamedPipe { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the owner can read from this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if owner can read from this file; otherwise, <see langword="false"/>.
        /// </value>
        bool OwnerCanRead { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the owner can write into this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if owner can write into this file; otherwise, <see langword="false"/>.
        /// </value>
        bool OwnerCanWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the owner can execute this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if owner can execute this file; otherwise, <see langword="false"/>.
        /// </value>
        bool OwnerCanExecute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group members can read from this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if group members can read from this file; otherwise, <see langword="false"/>.
        /// </value>
        bool GroupCanRead { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group members can write into this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if group members can write into this file; otherwise, <see langword="false"/>.
        /// </value>
        bool GroupCanWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group members can execute this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if group members can execute this file; otherwise, <see langword="false"/>.
        /// </value>
        bool GroupCanExecute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the others can read from this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if others can read from this file; otherwise, <see langword="false"/>.
        /// </value>
        bool OthersCanRead { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the others can write into this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if others can write into this file; otherwise, <see langword="false"/>.
        /// </value>
        bool OthersCanWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the others can execute this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if others can execute this file; otherwise, <see langword="false"/>.
        /// </value>
        bool OthersCanExecute { get; set; }

        /// <summary>
        /// Sets file  permissions.
        /// </summary>
        /// <param name="mode">The mode.</param>
        void SetPermissions(short mode);

        /// <summary>
        /// Permanently deletes a file on remote machine.
        /// </summary>
        void Delete();

        /// <summary>
        /// Moves a specified file to a new location on remote machine, providing the option to specify a new file name.
        /// </summary>
        /// <param name="destFileName">The path to move the file to, which can specify a different file name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="destFileName"/> is <see langword="null"/>.</exception>
        void MoveTo(string destFileName);

        /// <summary>
        /// Updates file status on the server.
        /// </summary>
        void UpdateStatus();
    }
}
