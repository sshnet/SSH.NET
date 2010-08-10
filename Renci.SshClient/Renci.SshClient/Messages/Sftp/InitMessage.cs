namespace Renci.SshClient.Messages.Sftp
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

        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Version);
        }
    }
}
