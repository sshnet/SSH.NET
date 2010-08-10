
using System.Text;
namespace Renci.SshClient.Messages.Sftp
{
    internal class ReadLinkMessage : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.ReadLink; }
        }

        public string Path { get; set; }

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
