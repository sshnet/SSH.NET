
namespace Renci.SshClient.Sftp.Messages
{
    internal class ReadDirMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.ReadDir; }
        }

        public string Handle { get; set; }

        public ReadDirMessage()
        {

        }

        public ReadDirMessage(string handle)
        {
            this.Handle = handle;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Handle);
        }
    }
}
