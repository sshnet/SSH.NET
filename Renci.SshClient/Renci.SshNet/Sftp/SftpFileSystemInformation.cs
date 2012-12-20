namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Contains File system information exposed by statvfs@openssh.com request.
    /// </summary>
    public class SftpFileSytemInformation
    {
        private ulong _flag;

        private const ulong SSH_FXE_STATVFS_ST_RDONLY = 0x1;

        private const ulong SSH_FXE_STATVFS_ST_NOSUID = 0x2;

        public ulong BlockSize { get; private set; }

        public ulong TotalBlocks { get; private set; }

        public ulong FreeBlocks { get; private set; }

        public ulong AvailableBlocks { get; private set; }

        public ulong TotalNodes { get; private set; }

        public ulong FreeNodes { get; private set; }

        public ulong AvailableNodes { get; private set; }

        public ulong Sid { get; private set; }

        public bool IsReadOnly
        {
            get { return (_flag & SSH_FXE_STATVFS_ST_RDONLY) == SSH_FXE_STATVFS_ST_RDONLY; }
        }

        public bool SupportsSetUid
        {
            get { return (_flag & SSH_FXE_STATVFS_ST_NOSUID) == 0; }
        }

        public ulong MaxNameLenght { get; private set; }

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