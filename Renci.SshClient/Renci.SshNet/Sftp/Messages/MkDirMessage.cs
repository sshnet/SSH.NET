namespace Renci.SshNet.Sftp.Messages
{
    internal class MkDirMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.MkDir; }
        }

        public string Path { get; private set; }

        public SftpFileAttributes Attributes { get; private set; }

        public MkDirMessage()
        {
            this.Attributes = SftpFileAttributes.Empty;
        }

        public MkDirMessage(uint requestId, string path)
            : base(requestId)
        {
            this.Path = path;
            this.Attributes = SftpFileAttributes.Empty;
        }

        public MkDirMessage(uint requestId, string path, SftpFileAttributes attributes)
            : base(requestId)
        {
            this.Path = path;
            this.Attributes = attributes;
        }

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
