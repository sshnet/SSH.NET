
namespace Renci.SshClient.Messages.Sftp
{
    internal class LStatMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.LStat; }
        }

        public string Path { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.Path = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Path);
        }
    }
}
