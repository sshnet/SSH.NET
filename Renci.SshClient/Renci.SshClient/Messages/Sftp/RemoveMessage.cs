
using System.Text;
namespace Renci.SshClient.Messages.Sftp
{
    internal class RemoveMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Remove; }
        }

        public string Filename { get; set; }

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
