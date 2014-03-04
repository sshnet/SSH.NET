namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpDataResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Data; }
        }

        public byte[] Data { get; set; }

        public bool IsEof { get; set; }

        public SftpDataResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        protected override void LoadData()
        {
            base.LoadData();
            
            this.Data = this.ReadBinaryString();

            if (!this.IsEndOfData)
            {
                this.IsEof = this.ReadBoolean();
            }
        }
    }
}
