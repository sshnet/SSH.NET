
namespace Renci.SshClient.Messages.Sftp
{
    internal class DataMessage : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Data; }
        }

        public string Data { get; set; }

        public bool IsEof { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.Data = this.ReadString();
            if (!this.IsEndOfData)
            {
                this.IsEof = this.ReadBoolean();
            }
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Data);
            if (this.IsEof)
            {
                this.Write(this.IsEof);
            }
        }
    }
}
