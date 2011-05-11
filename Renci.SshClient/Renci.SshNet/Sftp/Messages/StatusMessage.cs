namespace Renci.SshNet.Sftp.Messages
{
    internal class StatusMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Status; }
        }

        public StatusCodes StatusCode { get; private set; }

        public string ErrorMessage { get; private set; }

        public string Language { get; private set; }

        public StatusMessage()
        {

        }

        public StatusMessage(uint requestId, StatusCodes statusCode, string errorMessage, string language)
            : base(requestId)
        {
            this.StatusCode = statusCode;
            this.ErrorMessage = errorMessage;
            this.Language = language;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.StatusCode = (StatusCodes)this.ReadUInt32();

            if (!this.IsEndOfData)
            {
                this.ErrorMessage = this.ReadString();
                this.Language = this.ReadString();
            }
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write((uint)this.StatusCode);
            if (!string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.Write(this.ErrorMessage);
                this.Write(this.Language);
            }
        }
    }
}
