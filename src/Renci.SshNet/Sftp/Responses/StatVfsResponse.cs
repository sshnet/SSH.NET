namespace Renci.SshNet.Sftp.Responses
{
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