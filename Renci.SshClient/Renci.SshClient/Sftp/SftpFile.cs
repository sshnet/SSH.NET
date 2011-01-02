using System;
using System.Collections.Generic;
using Renci.SshClient.Sftp.Messages;
using System.Security;
using System.IO;
using System.Security.AccessControl;

namespace Renci.SshClient.Sftp
{
    /// <summary>
    /// Represents SFTP file information
    /// </summary>
    public class SftpFile
    {
        #region Bitmask constats

        private static UInt32 S_IFMT = 0xF000;  //  bitmask for the file type bitfields

        private static UInt32 S_IFSOCK = 0xC000;  //	socket

        private static UInt32 S_IFLNK = 0xA000;  //	symbolic link

        private static UInt32 S_IFREG = 0x8000;  //	regular file

        private static UInt32 S_IFBLK = 0x6000;  //	block device

        private static UInt32 S_IFDIR = 0x4000;  //	directory

        private static UInt32 S_IFCHR = 0x2000;  //	character device

        private static UInt32 S_IFIFO = 0x1000;  //	FIFO

        private static UInt32 S_ISUID = 0x0800;  //	set UID bit

        private static UInt32 S_ISGID = 0x0400;  //	set-group-ID bit (see below)

        private static UInt32 S_ISVTX = 0x0200;  //	sticky bit (see below)

        private static UInt32 S_IRWXU = 0x01C0;  //	mask for file owner permissions

        private static UInt32 S_IRUSR = 0x0100;  //	owner has read permission

        private static UInt32 S_IWUSR = 0x0080;  //	owner has write permission

        private static UInt32 S_IXUSR = 0x0040;  //	owner has execute permission

        private static UInt32 S_IRWXG = 0x0038;  //	mask for group permissions

        private static UInt32 S_IRGRP = 0x0020;  //	group has read permission

        private static UInt32 S_IWGRP = 0x0010;  //	group has write permission

        private static UInt32 S_IXGRP = 0x0008;  //	group has execute permission

        private static UInt32 S_IRWXO = 0x0007;  //	mask for permissions for others (not in group)

        private static UInt32 S_IROTH = 0x0004;  //	others have read permission

        private static UInt32 S_IWOTH = 0x0002;  //	others have write permission

        private static UInt32 S_IXOTH = 0x0001;  //	others have execute permission

        #endregion

        private UInt32 _permissions;

        private Attributes _attributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpFile"/> class.
        /// </summary>
        /// <param name="absolutePath">Name of the file.</param>
        /// <param name="attributes">The attributes.</param>
        public SftpFile(string absolutePath, Attributes attributes)
        {
            this.AbsolutePath = absolutePath;

            this._permissions = attributes.Permissions;

            this._attributes = attributes;
        }


        /// <summary>
        /// Gets the absolute path of the directory or file.
        /// </summary>
        /// <value>
        /// File name.
        /// </value>
        public string AbsolutePath { get; private set; }

        ///// <summary>
        ///// Gets the name of directory or file.
        ///// </summary>
        //public string Name { get; private set; }

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
                if (this._attributes.AccessTime.HasValue)
                    return this._attributes.AccessTime.Value;
                else
                    return DateTime.MinValue;
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
                if (this._attributes.ModifyTime.HasValue)
                    return this._attributes.ModifyTime.Value;
                else
                    return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets or sets the size, in bytes, of the current file.
        /// </summary>
        /// <value>
        /// The size of the current file in bytes.
        /// </value>
        public long Size
        {
            get
            {
                if (this._attributes.Size.HasValue)
                    return (long)this._attributes.Size.Value;
                else
                    return -1;
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
                if (this._attributes.UserId.HasValue)
                    return (int)this._attributes.UserId.Value;
                else
                    return -1;
            }
            set
            {
                if (value > -1)
                {
                    this._attributes.UserId = new Nullable<uint>((uint)value);
                }
                else
                {
                    this._attributes.UserId = null;
                }
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
                if (this._attributes.GroupId.HasValue)
                    return (int)this._attributes.GroupId.Value;
                else
                    return -1;
            }
            set
            {
                if (value > -1)
                {
                    this._attributes.GroupId = new Nullable<uint>((uint)value);
                }
                else
                {
                    this._attributes.GroupId = null;
                }
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
                return ((this._permissions & S_IFSOCK) == S_IFSOCK);
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
                return ((this._permissions & S_IFLNK) == S_IFLNK);
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
                return ((this._permissions & S_IFBLK) == S_IFBLK);
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
                return ((this._permissions & S_IFDIR) == S_IFDIR);
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
                return ((this._permissions & S_IFCHR) == S_IFCHR);
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
                return ((this._permissions & S_IFIFO) == S_IFIFO);
            }
        }


        /// <summary>
        /// Gets a value indicating whether the owner can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanRead
        {
            get
            {
                return ((this._permissions & S_IRUSR) == S_IRUSR);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the owner can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanWrite
        {
            get
            {
                return ((this._permissions & S_IWUSR) == S_IWUSR);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the owner can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanExecute
        {
            get
            {
                return ((this._permissions & S_IXUSR) == S_IXUSR);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the group members can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanRead
        {
            get
            {
                return ((this._permissions & S_IRUSR) == S_IRGRP);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the group members can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanWrite
        {
            get
            {
                return ((this._permissions & S_IWGRP) == S_IWGRP);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the group members can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanExecute
        {
            get
            {
                return ((this._permissions & S_IXGRP) == S_IXGRP);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the others can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanRead
        {
            get
            {
                return ((this._permissions & S_IROTH) == S_IROTH);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the others can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanWrite
        {
            get
            {
                return ((this._permissions & S_IWOTH) == S_IWOTH);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the others can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanExecute
        {
            get
            {
                return ((this._permissions & S_IXOTH) == S_IXOTH);
            }
        }

        /// <summary>
        /// Gets the extension part of the file.
        /// </summary>
        /// <value>
        /// File extensions.
        /// </value>
        public IDictionary<string, string> Extensions
        {
            get
            {
                return this._attributes.Extensions;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Name {0}, Size {1}, User ID {2}, Group ID {3}, Permissions {4:X}, Accessed {5}, Modified {6}", this.AbsolutePath, this.Size, this.UserId, this.GroupId, this._permissions, this.LastAccessTime, this.LastWriteTime);
        }
    }
}
