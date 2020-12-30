using System;
using System.Globalization;
using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Represents SFTP file information
    /// </summary>
    public class SftpFile
    {
        private readonly ISftpSession _sftpSession;

        /// <summary>
        /// Gets the file attributes.
        /// </summary>
        public SftpFileAttributes Attributes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpFile"/> class.
        /// </summary>
        /// <param name="sftpSession">The SFTP session.</param>
        /// <param name="fullName">Full path of the directory or file.</param>
        /// <param name="attributes">Attributes of the directory or file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sftpSession"/> or <paramref name="fullName"/> is <c>null</c>.</exception>
        internal SftpFile(ISftpSession sftpSession, string fullName, SftpFileAttributes attributes)
        {
            if (sftpSession == null)
                throw new SshConnectionException("Client not connected.");

            if (attributes == null)
                throw new ArgumentNullException("attributes");

            if (fullName == null)
                throw new ArgumentNullException("fullName");

            _sftpSession = sftpSession;
            Attributes = attributes;

            Name = fullName.Substring(fullName.LastIndexOf('/') + 1);

            FullName = fullName;
        }

        /// <summary>
        /// Gets the full path of the directory or file.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// For files, gets the name of the file. For directories, gets the name of the last directory in the hierarchy if a hierarchy exists. 
        /// Otherwise, the Name property gets the name of the directory.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the time the current file or directory was last accessed.
        /// </summary>
        /// <value>
        /// The time that the current file or directory was last accessed.
        /// </value>
        public DateTime LastAccessTime
        {
            get
            {
                return Attributes.LastAccessTime;
            }
            set
            {
                Attributes.LastAccessTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the time when the current file or directory was last written to.
        /// </summary>
        /// <value>
        /// The time the current file was last written.
        /// </value>
        public DateTime LastWriteTime
        {
            get
            {
                return Attributes.LastWriteTime;
            }
            set
            {
                Attributes.LastWriteTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the time, in coordinated universal time (UTC), the current file or directory was last accessed.
        /// </summary>
        /// <value>
        /// The time that the current file or directory was last accessed.
        /// </value>
        public DateTime LastAccessTimeUtc
        {
            get
            {
                return Attributes.LastAccessTimeUtc;
            }
            set
            {
                Attributes.LastAccessTimeUtc = value;
            }
        }

        /// <summary>
        /// Gets or sets the time, in coordinated universal time (UTC), when the current file or directory was last written to.
        /// </summary>
        /// <value>
        /// The time the current file was last written.
        /// </value>
        public DateTime LastWriteTimeUtc
        {
            get
            {
                return Attributes.LastWriteTimeUtc;
            }
            set
            {
                Attributes.LastWriteTimeUtc = value;
            }
        }

        /// <summary>
        /// Gets or sets the size, in bytes, of the current file.
        /// </summary>
        /// <value>
        /// The size of the current file in bytes.
        /// </value>
        public long Length
        {
            get
            {
                return Attributes.Size;
            }
        }

        /// <summary>
        /// Gets or sets file user id.
        /// </summary>
        /// <value>
        /// File user id.
        /// </value>
        public int UserId
        {
            get
            {
                return Attributes.UserId;
            }
            set
            {
                Attributes.UserId = value;
            }
        }

        /// <summary>
        /// Gets or sets file group id.
        /// </summary>
        /// <value>
        /// File group id.
        /// </value>
        public int GroupId
        {
            get
            {
                return Attributes.GroupId;
            }
            set
            {
                Attributes.GroupId = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a socket.
        /// </summary>
        /// <value>
        ///   <c>true</c> if file represents a socket; otherwise, <c>false</c>.
        /// </value>
        public bool IsSocket
        {
            get
            {
                return Attributes.IsSocket;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a symbolic link.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a symbolic link; otherwise, <c>false</c>.
        /// </value>
        public bool IsSymbolicLink
        {
            get
            {
                return Attributes.IsSymbolicLink;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a regular file.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a regular file; otherwise, <c>false</c>.
        /// </value>
        public bool IsRegularFile
        {
            get
            {
                return Attributes.IsRegularFile;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a block device.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a block device; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockDevice
        {
            get
            {
                return Attributes.IsBlockDevice;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a directory.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a directory; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectory
        {
            get
            {
                return Attributes.IsDirectory;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a character device.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a character device; otherwise, <c>false</c>.
        /// </value>
        public bool IsCharacterDevice
        {
            get
            {
                return Attributes.IsCharacterDevice;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a named pipe.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a named pipe; otherwise, <c>false</c>.
        /// </value>
        public bool IsNamedPipe
        {
            get
            {
                return Attributes.IsNamedPipe;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the owner can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanRead
        {
            get
            {
                return Attributes.OwnerCanRead;
            }
            set
            {
                Attributes.OwnerCanRead = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the owner can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanWrite
        {
            get
            {
                return Attributes.OwnerCanWrite;
            }
            set
            {
                Attributes.OwnerCanWrite = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the owner can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanExecute
        {
            get
            {
                return Attributes.OwnerCanExecute;
            }
            set
            {
                Attributes.OwnerCanExecute = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the group members can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanRead
        {
            get
            {
                return Attributes.GroupCanRead;
            }
            set
            {
                Attributes.GroupCanRead = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the group members can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanWrite
        {
            get
            {
                return Attributes.GroupCanWrite;
            }
            set
            {
                Attributes.GroupCanWrite = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the group members can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanExecute
        {
            get
            {
                return Attributes.GroupCanExecute;
            }
            set
            {
                Attributes.GroupCanExecute = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the others can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanRead
        {
            get
            {
                return Attributes.OthersCanRead;
            }
            set
            {
                Attributes.OthersCanRead = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the others can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanWrite
        {
            get
            {
                return Attributes.OthersCanWrite;
            }
            set
            {
                Attributes.OthersCanWrite = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the others can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanExecute
        {
            get
            {
                return Attributes.OthersCanExecute;
            }
            set
            {
                Attributes.OthersCanExecute = value;
            }
        }

        /// <summary>
        /// Sets file  permissions.
        /// </summary>
        /// <param name="mode">The mode.</param>
        public void SetPermissions(short mode)
        {
            Attributes.SetPermissions(mode);

            UpdateStatus();
        }

        /// <summary>
        /// Permanently deletes a file on remote machine.
        /// </summary>
        public void Delete()
        {
            if (IsDirectory)
            {
                _sftpSession.RequestRmDir(FullName);
            }
            else
            {
                _sftpSession.RequestRemove(FullName);
            }
        }

        /// <summary>
        /// Moves a specified file to a new location on remote machine, providing the option to specify a new file name.
        /// </summary>
        /// <param name="destFileName">The path to move the file to, which can specify a different file name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="destFileName"/> is <c>null</c>.</exception>
        public void MoveTo(string destFileName)
        {
            if (destFileName == null)
                throw new ArgumentNullException("destFileName");
            _sftpSession.RequestRename(FullName, destFileName);

            var fullPath = _sftpSession.GetCanonicalPath(destFileName);

            Name = fullPath.Substring(fullPath.LastIndexOf('/') + 1);

            FullName = fullPath;
        }

        /// <summary>
        /// Updates file status on the server.
        /// </summary>
        public void UpdateStatus()
        {
            _sftpSession.RequestSetStat(FullName, Attributes);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Name {0}, Length {1}, User ID {2}, Group ID {3}, Accessed {4}, Modified {5}", Name, Length, UserId, GroupId, LastAccessTime, LastWriteTime);
        }
    }
}
