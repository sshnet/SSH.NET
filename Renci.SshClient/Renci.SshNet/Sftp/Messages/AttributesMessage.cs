namespace Renci.SshNet.Sftp.Messages
{
    internal class AttributesMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Attrs; }
        }

        public SftpFileAttributes Attributes { get; private set; }

        public AttributesMessage()
        {

        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Attributes = this.ReadAttributes();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Attributes);
        }
    }
}
