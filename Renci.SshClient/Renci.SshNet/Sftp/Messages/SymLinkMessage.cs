using System.Text;

namespace Renci.SshNet.Sftp.Messages
{
    internal class SymLinkMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.SymLink; }
        }

        public string NewLinkPath { get; set; }

        public string ExistingPath { get; set; }

        public SymLinkMessage()
        {

        }

        public SymLinkMessage(uint requestId, string newLinkPath, string existingPath)
            : base(requestId)
        {
            this.NewLinkPath = newLinkPath;
            this.ExistingPath = existingPath;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.NewLinkPath = this.ReadString();
            this.ExistingPath = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.NewLinkPath, Encoding.UTF8);
            this.Write(this.ExistingPath, Encoding.UTF8);
        }
    }
}
