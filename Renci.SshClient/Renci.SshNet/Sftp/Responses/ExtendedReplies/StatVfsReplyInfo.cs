namespace Renci.SshNet.Sftp.Responses
{
    internal class StatVfsReplyInfo : ExtendedReplyInfo
    {
        public SftpFileSytemInformation Information { get; private set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.Information = new SftpFileSytemInformation(this.ReadUInt64(), this.ReadUInt64(),
                                                                     this.ReadUInt64(), this.ReadUInt64(),
                                                                     this.ReadUInt64(), this.ReadUInt64(),
                                                                     this.ReadUInt64(), this.ReadUInt64(),
                                                                     this.ReadUInt64(), this.ReadUInt64(),
                                                                     this.ReadUInt64());
        }

        protected override void SaveData()
        {
            throw new System.NotImplementedException();
        }
    }
}