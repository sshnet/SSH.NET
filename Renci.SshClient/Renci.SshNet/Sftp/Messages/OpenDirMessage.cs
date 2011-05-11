using System.Text;

namespace Renci.SshNet.Sftp.Messages
{
    internal class OpenDirMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.OpenDir; }
        }

        public string Path { get; private set; }

        public OpenDirMessage()
        {

        }

        public OpenDirMessage(uint requestId, string path)
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
