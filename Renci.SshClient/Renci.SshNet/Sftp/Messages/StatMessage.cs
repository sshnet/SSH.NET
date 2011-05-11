
namespace Renci.SshNet.Sftp.Messages
{
    internal class StatMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Stat; }
        }

        public string Path { get; private set; }

        public StatMessage()
        {

        }

        public StatMessage(uint requestId, string path)
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
