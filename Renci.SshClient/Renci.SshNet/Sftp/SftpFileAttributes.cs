using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Contains SFTP file attributes.
    /// </summary>
    public class SftpFileAttributes
    {
        #region Bitmask constats

        private const UInt32 S_IFMT = 0xF000; //  bitmask for the file type bitfields

        private const UInt32 S_IFSOCK = 0xC000; //	socket

        private const UInt32 S_IFLNK = 0xA000; //	symbolic link

        private const UInt32 S_IFREG = 0x8000; //	regular file

        private const UInt32 S_IFBLK = 0x6000; //	block device

        private const UInt32 S_IFDIR = 0x4000; //	directory

        private const UInt32 S_IFCHR = 0x2000; //	character device

        private const UInt32 S_IFIFO = 0x1000; //	FIFO

        private const UInt32 S_ISUID = 0x0800; //	set UID bit

        private const UInt32 S_ISGID = 0x0400; //	set-group-ID bit (see below)

        private const UInt32 S_ISVTX = 0x0200; //	sticky bit (see below)

        private const UInt32 S_IRUSR = 0x0100; //	owner has read permission

        private const UInt32 S_IWUSR = 0x0080; //	owner has write permission

        private const UInt32 S_IXUSR = 0x0040; //	owner has execute permission

        private const UInt32 S_IRGRP = 0x0020; //	group has read permission

        private const UInt32 S_IWGRP = 0x0010; //	group has write permission

        private const UInt32 S_IXGRP = 0x0008; //	group has execute permission

        private const UInt32 S_IROTH = 0x0004; //	others have read permission

        private const UInt32 S_IWOTH = 0x0002; //	others have write permission

        private const UInt32 S_IXOTH = 0x0001; //	others have execute permission

        #endregion

        private bool _isBitFiledsBitSet;
        private bool _isUIDBitSet;
        private bool _isGroupIDBitSet;
        private bool _isStickyBitSet;

        private readonly DateTime _originalLastAccessTime;
        private readonly DateTime _originalLastWriteTime;
        private readonly long _originalSize;
        private readonly int _originalUserId;
        private readonly int _originalGroupId;
        private readonly uint _originalPermissions;
        private readonly IDictionary<string, string> _originalExtensions;

        internal bool IsLastAccessTimeChanged
        {
            get { return this._originalLastAccessTime != this.LastAccessTime; }
        }

        internal bool IsLastWriteTimeChanged
        {
            get { return this._originalLastWriteTime != this.LastWriteTime; }
        }

        internal bool IsSizeChanged
        {
            get { return this._originalSize != this.Size; }
        }

        internal bool IsUserIdChanged
        {
            get { return this._originalUserId != this.UserId; }
        }

        internal bool IsGroupIdChanged
        {
            get { return this._originalGroupId != this.GroupId; }
        }

        internal bool IsPermissionsChanged
        {
            get { return this._originalPermissions != this.Permissions; }
        }

        internal bool IsExtensionsChanged
        {
            get { return this._originalExtensions != null && this.Extensions != null && !this._originalExtensions.SequenceEqual(this.Extensions); }
        }

        /// <summary>
        /// Gets or sets the time the current file or directory was last accessed.
        /// </summary>
        /// <value>
        /// The time that the current file or directory was last accessed.
        /// </value>
        public DateTime LastAccessTime { get; set; }

        /// <summary>
        /// Gets or sets the time when the current file or directory was last written to.
        /// </summary>
        /// <value>
        /// The time the current file was last written.
        /// </value>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// Gets or sets the size, in bytes, of the current file.
        /// </summary>
        /// <value>
        /// The size of the current file in bytes.
        /// </value>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets file user id.
        /// </summary>
        /// <value>
        /// File user id.
        /// </value>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets file group id.
        /// </summary>
        /// <value>
        /// File group id.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets a value indicating whether file represents a socket.
        /// </summary>
        /// <value>
        ///   <c>true</c> if file represents a socket; otherwise, <c>false</c>.
        /// </value>
        public bool IsSocket { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a symbolic link.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a symbolic link; otherwise, <c>false</c>.
        /// </value>
        public bool IsSymbolicLink { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a regular file.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a regular file; otherwise, <c>false</c>.
        /// </value>
        public bool IsRegularFile { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a block device.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a block device; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockDevice { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a directory.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a directory; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectory { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a character device.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a character device; otherwise, <c>false</c>.
        /// </value>
        public bool IsCharacterDevice { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a named pipe.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a named pipe; otherwise, <c>false</c>.
        /// </value>
        public bool IsNamedPipe { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the owner can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanRead { get; set; }

        /// <summary>
        /// Gets a value indicating whether the owner can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanWrite { get; set; }

        /// <summary>
        /// Gets a value indicating whether the owner can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanExecute { get; set; }

        /// <summary>
        /// Gets a value indicating whether the group members can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanRead { get; set; }

        /// <summary>
        /// Gets a value indicating whether the group members can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanWrite { get; set; }

        /// <summary>
        /// Gets a value indicating whether the group members can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanExecute { get; set; }

        /// <summary>
        /// Gets a value indicating whether the others can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanRead { get; set; }

        /// <summary>
        /// Gets a value indicating whether the others can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanWrite { get; set; }

        /// <summary>
        /// Gets a value indicating whether the others can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanExecute { get; set; }

        /// <summary>
        /// Gets or sets the extensions.
        /// </summary>
        /// <value>
        /// The extensions.
        /// </value>
        public IDictionary<string, string> Extensions { get; private set; }

        internal uint Permissions
        {
            get
            {
                uint permission = 0;

                if (this._isBitFiledsBitSet)
                    permission = permission | S_IFMT;

                if (this.IsSocket)
                    permission = permission | S_IFSOCK;

                if (this.IsSymbolicLink)
                    permission = permission | S_IFLNK;

                if (this.IsRegularFile)
                    permission = permission | S_IFREG;

                if (this.IsBlockDevice)
                    permission = permission | S_IFBLK;

                if (this.IsDirectory)
                    permission = permission | S_IFDIR;

                if (this.IsCharacterDevice)
                    permission = permission | S_IFCHR;

                if (this.IsNamedPipe)
                    permission = permission | S_IFIFO;

                if (this._isUIDBitSet)
                    permission = permission | S_ISUID;

                if (this._isGroupIDBitSet)
                    permission = permission | S_ISGID;

                if (this._isStickyBitSet)
                    permission = permission | S_ISVTX;

                if (this.OwnerCanRead)
                    permission = permission | S_IRUSR;

                if (this.OwnerCanWrite)
                    permission = permission | S_IWUSR;

                if (this.OwnerCanExecute)
                    permission = permission | S_IXUSR;

                if (this.GroupCanRead)
                    permission = permission | S_IRGRP;

                if (this.GroupCanWrite)
                    permission = permission | S_IWGRP;

                if (this.GroupCanExecute)
                    permission = permission | S_IXGRP;

                if (this.OthersCanRead)
                    permission = permission | S_IROTH;

                if (this.OthersCanWrite)
                    permission = permission | S_IWOTH;

                if (this.OthersCanExecute)
                    permission = permission | S_IXOTH;

                return permission;
            }
            private set
            {
                this._isBitFiledsBitSet = ((value & S_IFMT) == S_IFMT);

                this.IsSocket = ((value & S_IFSOCK) == S_IFSOCK);

                this.IsSymbolicLink = ((value & S_IFLNK) == S_IFLNK);

                this.IsRegularFile = ((value & S_IFREG) == S_IFREG);

                this.IsBlockDevice = ((value & S_IFBLK) == S_IFBLK);

                this.IsDirectory = ((value & S_IFDIR) == S_IFDIR);

                this.IsCharacterDevice = ((value & S_IFCHR) == S_IFCHR);

                this.IsNamedPipe = ((value & S_IFIFO) == S_IFIFO);

                this._isUIDBitSet = ((value & S_ISUID) == S_ISUID);

                this._isGroupIDBitSet = ((value & S_ISGID) == S_ISGID);

                this._isStickyBitSet = ((value & S_ISVTX) == S_ISVTX);

                this.OwnerCanRead = ((value & S_IRUSR) == S_IRUSR);

                this.OwnerCanWrite = ((value & S_IWUSR) == S_IWUSR);

                this.OwnerCanExecute = ((value & S_IXUSR) == S_IXUSR);

                this.GroupCanRead = ((value & S_IRGRP) == S_IRGRP);

                this.GroupCanWrite = ((value & S_IWGRP) == S_IWGRP);

                this.GroupCanExecute = ((value & S_IXGRP) == S_IXGRP);

                this.OthersCanRead = ((value & S_IROTH) == S_IROTH);

                this.OthersCanWrite = ((value & S_IWOTH) == S_IWOTH);

                this.OthersCanExecute = ((value & S_IXOTH) == S_IXOTH);
            }
        }

        internal SftpFileAttributes()
        {
        }

        internal SftpFileAttributes(DateTime lastAccessTime, DateTime lastWriteTime, long size, int userId, int groupId, uint permissions, IDictionary<string, string> extensions)
        {
            this.LastAccessTime = this._originalLastAccessTime = lastAccessTime;
            this.LastWriteTime = this._originalLastWriteTime = lastWriteTime;
            this.Size = this._originalSize = size;
            this.UserId = this._originalUserId = userId;
            this.GroupId = this._originalGroupId = groupId;
            this.Permissions = this._originalPermissions = permissions;
            this.Extensions = this._originalExtensions = extensions;
        }

        /// <summary>
        /// Sets the permissions.
        /// </summary>
        /// <param name="mode">The mode.</param>
        public void SetPermissions(short mode)
        {
            if (mode < 0 || mode > 999)
            {
                throw new ArgumentOutOfRangeException("mode");
            }

            var modeBytes = mode.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0').ToArray();

            var permission = (modeBytes[0] & 0x0F) * 8 * 8 + (modeBytes[1] & 0x0F) * 8 + (modeBytes[2] & 0x0F);

            this.OwnerCanRead = (permission & S_IRUSR) == S_IRUSR;
            this.OwnerCanWrite = (permission & S_IWUSR) == S_IWUSR;
            this.OwnerCanExecute = (permission & S_IXUSR) == S_IXUSR;

            this.GroupCanRead = (permission & S_IRGRP) == S_IRGRP;
            this.GroupCanWrite = (permission & S_IWGRP) == S_IWGRP;
            this.GroupCanExecute = (permission & S_IXGRP) == S_IXGRP;

            this.OthersCanRead = (permission & S_IROTH) == S_IROTH;
            this.OthersCanWrite = (permission & S_IWOTH) == S_IWOTH;
            this.OthersCanExecute = (permission & S_IXOTH) == S_IXOTH;
        }
    }
}
