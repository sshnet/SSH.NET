
namespace Renci.SshNet.Sftp.Messages
{
    internal class LStatMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.LStat; }
        }

        public string Path { get; private set; }

        public LStatMessage()
        {
                
        }

        public LStatMessage(uint requestId, string path)
            : base(requestId)
        {
            this.Path = path;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Path = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Path);
        }
    }
}
