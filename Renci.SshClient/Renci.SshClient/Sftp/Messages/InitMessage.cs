namespace Renci.SshClient.Sftp.Messages
{
    internal class InitMessage : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Init; }
        }

        public uint Version { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            this.Version = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Version);
        }
    }
}
