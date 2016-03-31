namespace Renci.SshNet.Sftp.Responses
{
    internal class StatVfsResponse : SftpExtendedReplyResponse
    {
        public SftpFileSytemInformation Information { get; private set; }

        public StatVfsResponse()
            : base(0)
        {
        }

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
    }
}