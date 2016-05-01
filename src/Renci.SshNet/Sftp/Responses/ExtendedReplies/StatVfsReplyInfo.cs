namespace Renci.SshNet.Sftp.Responses
{
    internal class StatVfsReplyInfo : ExtendedReplyInfo
    {
        public SftpFileSytemInformation Information { get; private set; }

        protected override void LoadData()
        {
            base.LoadData();

            Information = new SftpFileSytemInformation(ReadUInt64(),
                                                       ReadUInt64(),
                                                       ReadUInt64(),
                                                       ReadUInt64(),
                                                       ReadUInt64(),
                                                       ReadUInt64(),
                                                       ReadUInt64(),
                                                       ReadUInt64(),
                                                       ReadUInt64(),
                                                       ReadUInt64(),
                                                       ReadUInt64());
        }

        protected override void SaveData()
        {
            throw new System.NotImplementedException();
        }
    }
}