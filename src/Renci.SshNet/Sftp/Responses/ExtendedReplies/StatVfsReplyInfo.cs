using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp.Responses
{
    internal sealed class StatVfsReplyInfo : ExtendedReplyInfo
    {
        public SftpFileSystemInformation Information { get; private set; }

        public override void LoadData(SshDataStream stream)
        {
            Information = new SftpFileSystemInformation(stream.ReadUInt64(), // FileSystemBlockSize
                                                       stream.ReadUInt64(), // BlockSize
                                                       stream.ReadUInt64(), // TotalBlocks
                                                       stream.ReadUInt64(), // FreeBlocks
                                                       stream.ReadUInt64(), // AvailableBlocks
                                                       stream.ReadUInt64(), // TotalNodes
                                                       stream.ReadUInt64(), // FreeNodes
                                                       stream.ReadUInt64(), // AvailableNodes
                                                       stream.ReadUInt64(), // Sid
                                                       stream.ReadUInt64(), // Flags
                                                       stream.ReadUInt64()); // MaxNameLenght
        }
    }
}
