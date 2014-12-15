namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpDataResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Data; }
        }

        public byte[] Data { get; private set; }

        public bool IsEof { get; private set; }

        public SftpDataResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        protected override void LoadData()
        {
            base.LoadData();
            
#if TUNING
            this.Data = this.ReadBinary();
#else
            this.Data = this.ReadBinaryString();
#endif

            if (!this.IsEndOfData)
            {
                this.IsEof = this.ReadBoolean();
            }
        }
    }
}
