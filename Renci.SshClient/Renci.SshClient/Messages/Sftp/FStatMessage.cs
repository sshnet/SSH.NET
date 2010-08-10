
namespace Renci.SshClient.Messages.Sftp
{
    internal class FStatMessage : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.FStat; }
        }

        public string Handle { get; set; }

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
