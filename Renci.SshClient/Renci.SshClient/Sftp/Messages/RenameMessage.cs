
namespace Renci.SshClient.Sftp.Messages
{
    internal class RenameMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Rename; }
        }

        public string OldPath { get; private set; }

        public string NewPath { get; private set; }

        public RenameMessage()
        {

        }

        public RenameMessage(uint requestId, string oldPath, string newPath)
            : base(requestId)
        {
            this.OldPath = oldPath;
            this.NewPath = newPath;
        }

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
