using System.Linq;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Messages.Sftp
{
    internal class SftpDataMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelData; }
        }

        public SftpMessage Data { get; set; }

        protected override void SaveData()
        {
            base.SaveData();
            var data = this.Data.GetBytes();
            this.Write((uint)data.Count() + 4);
            this.Write(data.GetSshString());
        }
    }
}
