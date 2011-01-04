namespace Renci.SshClient.Sftp.Messages
{
    internal class MkDirMessage : SftpRequestMessage
    {
        public MkDirMessage()
        {
            this.Attributes = new SftpFileAttributes();
        }

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.MkDir; }
        }

        public string Path { get; set; }

        public SftpFileAttributes Attributes { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.Path = this.ReadString();
            this.Attributes = this.ReadAttributes();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Path);
            this.Write(this.Attributes);
        }

    }
}
