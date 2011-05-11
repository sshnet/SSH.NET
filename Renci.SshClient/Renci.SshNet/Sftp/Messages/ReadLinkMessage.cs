using System.Text;

namespace Renci.SshNet.Sftp.Messages
{
    internal class ReadLinkMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.ReadLink; }
        }

        public string Path { get; private set; }

        public ReadLinkMessage()
        {

        }

        public ReadLinkMessage(uint requestId, string path)
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
            this.Write(this.Path, Encoding.UTF8);
        }
    }
}
