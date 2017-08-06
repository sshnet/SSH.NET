using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    internal class SftpStatVfsResponseBuilder
    {
        private uint _protocolVersion;
        private uint _responseId;
        private ulong _bsize;
        private ulong _frsize;
        private ulong _blocks;
        private ulong _bfree;
        private ulong _bavail;
        private ulong _files;
        private ulong _ffree;
        private ulong _favail;
        private ulong _sid;
        private ulong _flag;
        private ulong _namemax;

        public SftpStatVfsResponseBuilder WithProtocolVersion(uint protocolVersion)
        {
            _protocolVersion = protocolVersion;
            return this;
        }

        public SftpStatVfsResponseBuilder WithResponseId(uint responseId)
        {
            _responseId = responseId;
            return this;
        }

        public SftpStatVfsResponseBuilder WithBSize(ulong bsize)
        {
            _bsize = bsize;
            return this;
        }

        public SftpStatVfsResponseBuilder WithFrSize(ulong frsize)
        {
            _frsize = frsize;
            return this;
        }

        public SftpStatVfsResponseBuilder WithBlocks(ulong blocks)
        {
            _blocks = blocks;
            return this;
        }

        public SftpStatVfsResponseBuilder WithBFree(ulong bfree)
        {
            _bfree = bfree;
            return this;
        }

        public SftpStatVfsResponseBuilder WithBAvail(ulong bavail)
        {
            _bavail = bavail;
            return this;
        }

        public SftpStatVfsResponseBuilder WithFiles(ulong files)
        {
            _files = files;
            return this;
        }

        public SftpStatVfsResponseBuilder WithFFree(ulong ffree)
        {
            _ffree = ffree;
            return this;
        }

        public SftpStatVfsResponseBuilder WithFAvail(ulong favail)
        {
            _favail = favail;
            return this;
        }

        public SftpStatVfsResponseBuilder WithSid(ulong sid)
        {
            _sid = sid;
            return this;
        }

        public SftpStatVfsResponseBuilder WithIsReadOnly(bool isReadOnly)
        {
            if (isReadOnly)
                _flag &= SftpFileSytemInformation.SSH_FXE_STATVFS_ST_RDONLY;
            else
                _flag |= SftpFileSytemInformation.SSH_FXE_STATVFS_ST_RDONLY;

            return this;
        }

        public SftpStatVfsResponseBuilder WithSupportsSetUid(bool supportsSetUid)
        {
            if (supportsSetUid)
                _flag |= SftpFileSytemInformation.SSH_FXE_STATVFS_ST_NOSUID;
            else
                _flag &= SftpFileSytemInformation.SSH_FXE_STATVFS_ST_NOSUID;

            return this;
        }

        public SftpStatVfsResponseBuilder WithNameMax(ulong nameMax)
        {
            _namemax = nameMax;
            return this;
        }

        public StatVfsResponse Build()
        {
            var fileSystemInfo = new SftpFileSytemInformation(_bsize,
                                                              _frsize,
                                                              _blocks,
                                                              _bfree,
                                                              _bavail,
                                                              _files,
                                                              _ffree,
                                                              _favail,
                                                              _sid,
                                                              _flag,
                                                              _namemax);

            return new StatVfsResponse(_protocolVersion)
            {
                ResponseId = _responseId,
                Information = fileSystemInfo
            };
        }
    }

    internal class StatVfsResponse : SftpExtendedReplyResponse
    {
        public SftpFileSytemInformation Information { get; set; }

        public StatVfsResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        protected override void LoadData()
        {
            base.LoadData();

            Information = new SftpFileSytemInformation(ReadUInt64(), // FileSystemBlockSize
                                                        ReadUInt64(), // BlockSize
                                                        ReadUInt64(), // TotalBlocks
                                                        ReadUInt64(), // FreeBlocks
                                                        ReadUInt64(), // AvailableBlocks
                                                        ReadUInt64(), // TotalNodes
                                                        ReadUInt64(), // FreeNodes
                                                        ReadUInt64(), // AvailableNodes
                                                        ReadUInt64(), // Sid
                                                        ReadUInt64(), // Flags
                                                        ReadUInt64()  // MaxNameLenght
                                                        );
        }

        protected override void SaveData()
        {
            base.SaveData();

            Information.SaveData(DataStream);
        }
    }
}
