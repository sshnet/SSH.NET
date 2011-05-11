namespace Renci.SshNet.Sftp.Messages
{
    internal class InitMessage : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Init; }
        }

        public uint Version { get; private set; }

        public InitMessage(uint version)
        {
            this.Version = version;
        }

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
