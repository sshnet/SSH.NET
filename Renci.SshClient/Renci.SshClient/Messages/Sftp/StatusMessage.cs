namespace Renci.SshClient.Messages.Sftp
{
    internal class StatusMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Status; }
        }

        public StatusCodes StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public string Language { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.StatusCode = (StatusCodes)this.ReadUInt32();

            switch (this.StatusCode)
            {
                case StatusCodes.Ok:
                    break;
                case StatusCodes.Eof:
                    break;
                case StatusCodes.NoSuchFile:
                    break;
                case StatusCodes.PermissionDenied:
                    break;
                case StatusCodes.Failure:
                    break;
                case StatusCodes.BadMessage:
                    break;
                case StatusCodes.NoConnection:
                    break;
                case StatusCodes.ConnectionLost:
                    break;
                case StatusCodes.OperationUnsupported:
                    break;
                default:
                    break;
            }

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
