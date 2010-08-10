using System.Text;

namespace Renci.SshClient.Messages.Sftp
{
    internal class SymLinkMessage : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.SymLink; }
        }

        public string NewLinkPath { get; set; }

        public string ExistingPath { get; set; }

        public bool IsSymLink { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.NewLinkPath = this.ReadString();
            this.ExistingPath = this.ReadString();
            this.IsSymLink = this.ReadBoolean();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.NewLinkPath, Encoding.UTF8);
            this.Write(this.ExistingPath, Encoding.UTF8);
            this.Write(this.IsSymLink);
        }
    }
}
