namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Contains File system information exposed by statvfs@openssh.com request.
    /// </summary>
    public class SftpFileSytemInformation
    {
        private readonly ulong _flag;

        private const ulong SSH_FXE_STATVFS_ST_RDONLY = 0x1;

        private const ulong SSH_FXE_STATVFS_ST_NOSUID = 0x2;

        /// <summary>
        /// Gets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        public ulong BlockSize { get; private set; }

        /// <summary>
        /// Gets the total blocks.
        /// </summary>
        /// <value>
        /// The total blocks.
        /// </value>
        public ulong TotalBlocks { get; private set; }

        /// <summary>
        /// Gets the free blocks.
        /// </summary>
        /// <value>
        /// The free blocks.
        /// </value>
        public ulong FreeBlocks { get; private set; }

        /// <summary>
        /// Gets the available blocks.
        /// </summary>
        /// <value>
        /// The available blocks.
        /// </value>
        public ulong AvailableBlocks { get; private set; }

        /// <summary>
        /// Gets the total nodes.
        /// </summary>
        /// <value>
        /// The total nodes.
        /// </value>
        public ulong TotalNodes { get; private set; }

        /// <summary>
        /// Gets the free nodes.
        /// </summary>
        /// <value>
        /// The free nodes.
        /// </value>
        public ulong FreeNodes { get; private set; }

        /// <summary>
        /// Gets the available nodes.
        /// </summary>
        /// <value>
        /// The available nodes.
        /// </value>
        public ulong AvailableNodes { get; private set; }

        /// <summary>
        /// Gets the sid.
        /// </summary>
        /// <value>
        /// The sid.
        /// </value>
        public ulong Sid { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return (_flag & SSH_FXE_STATVFS_ST_RDONLY) == SSH_FXE_STATVFS_ST_RDONLY; }
        }

        /// <summary>
        /// Gets a value indicating whether [supports set uid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports set uid]; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsSetUid
        {
            get { return (_flag & SSH_FXE_STATVFS_ST_NOSUID) == 0; }
        }

        /// <summary>
        /// Gets the max name lenght.
        /// </summary>
        /// <value>
        /// The max name lenght.
        /// </value>
        public ulong MaxNameLenght { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpFileSytemInformation" /> class.
        /// </summary>
        /// <param name="bsize">The bsize.</param>
        /// <param name="frsize">The frsize.</param>
        /// <param name="blocks">The blocks.</param>
        /// <param name="bfree">The bfree.</param>
        /// <param name="bavail">The bavail.</param>
        /// <param name="files">The files.</param>
        /// <param name="ffree">The ffree.</param>
        /// <param name="favail">The favail.</param>
        /// <param name="sid">The sid.</param>
        /// <param name="flag">The flag.</param>
        /// <param name="namemax">The namemax.</param>
        internal SftpFileSytemInformation(ulong bsize, ulong frsize, ulong blocks, ulong bfree, ulong bavail, ulong files, ulong ffree, ulong favail, ulong sid, ulong flag, ulong namemax)
        {
            this.BlockSize = frsize;
            this.TotalBlocks = blocks;
            this.FreeBlocks = bfree;
            this.AvailableBlocks = bavail;
            this.TotalNodes = files;
            this.FreeNodes = ffree;
            this.AvailableNodes = favail;
            this.Sid = sid;
            this._flag = flag;
            this.MaxNameLenght = namemax;
        }
    }
}