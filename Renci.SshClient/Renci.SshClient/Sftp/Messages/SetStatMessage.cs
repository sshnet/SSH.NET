
namespace Renci.SshClient.Sftp.Messages
{
    internal class SetStatMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.SetStat; }
        }

        public string Path { get; private set; }

        public SftpFileAttributes Attributes { get; private set; }

        public SetStatMessage()
        {

        }

        public SetStatMessage(uint requestId, string path, SftpFileAttributes attributes)
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
