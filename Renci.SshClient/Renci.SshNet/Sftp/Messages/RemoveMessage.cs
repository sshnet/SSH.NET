using System.Text;

namespace Renci.SshNet.Sftp.Messages
{
    internal class RemoveMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Remove; }
        }

        public string Filename { get; private set; }

        public RemoveMessage(uint requestId, string filename)
            : base(requestId)
        {
            this.Filename = filename;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Filename = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Filename, Encoding.UTF8);
        }
    }
}
