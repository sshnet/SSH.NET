
namespace Renci.SshNet.Sftp.Messages
{
    internal class RealPathMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.RealPath; }
        }

        public string Path { get; private set; }

        public RealPathMessage()
        {

        }

        public RealPathMessage(uint requestId, string path)
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
