
using System.Text;
namespace Renci.SshClient.Sftp.Messages
{
    internal class OpenDirMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.OpenDir; }
        }

        public string Path { get; set; }

        public OpenDirMessage()
        {

        }

        public OpenDirMessage(string path)
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
