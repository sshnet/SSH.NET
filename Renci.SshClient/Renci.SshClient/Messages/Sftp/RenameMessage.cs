
namespace Renci.SshClient.Messages.Sftp
{
    internal class RenameMessage : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Rename; }
        }

        public string OldPath { get; set; }

        public string NewPath { get; set; }
        protected override void LoadData()
        {
            base.LoadData();
            this.OldPath = this.ReadString();
            this.NewPath = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.OldPath);
            this.Write(this.NewPath);
        }
    }
}
