namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpStatusResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Status; }
        }

        public SftpStatusResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        public StatusCodes StatusCode { get; private set; }

        public string ErrorMessage { get; private set; }

        public string Language { get; private set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.StatusCode = (StatusCodes)this.ReadUInt32();

            if (this.ProtocolVersion < 3)
            {
                return;
            }

            if (!this.IsEndOfData)
            {
                this.ErrorMessage = this.ReadString();
                this.Language = this.ReadString();
            }
        }
    }
}
