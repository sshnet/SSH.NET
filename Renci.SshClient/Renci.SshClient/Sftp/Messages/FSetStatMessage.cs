namespace Renci.SshClient.Sftp.Messages
{
    internal class FSetStatMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.FSetStat; }
        }

        public byte[] Handle { get; private set; }

        public SftpFileAttributes Attributes { get; private set; }
        
        public FSetStatMessage()
        {

        }

        public FSetStatMessage(uint requestId, byte[] handle, SftpFileAttributes attributes)
            : base(requestId)
        {
            this.Handle = handle;
            this.Attributes = attributes;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadBinaryString();
            this.Attributes = this.ReadAttributes();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
            this.Write(this.Attributes);
        }
    }
}
