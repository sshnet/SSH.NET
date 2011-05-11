
namespace Renci.SshClient.Sftp.Messages
{
    internal class DataMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Data; }
        }

        public byte[] Data { get; set; }

        public bool IsEof { get; set; }

        public DataMessage()
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

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Data);
            if (this.IsEof)
            {
                this.Write(this.IsEof);
            }
        }
    }
}
