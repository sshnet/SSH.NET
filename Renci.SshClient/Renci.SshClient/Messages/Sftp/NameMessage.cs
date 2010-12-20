using System.Collections.Generic;
using Renci.SshClient.Sftp;

namespace Renci.SshClient.Messages.Sftp
{
    internal class NameMessage : SftpRequestMessage
    {

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Name; }
        }

        public uint Count { get; set; }

        public IEnumerable<SftpFile> Files { get; set; }

        public NameMessage()
        {
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Count = this.ReadUInt32();
            var files = new List<SftpFile>();
            for (int i = 0; i < this.Count; i++)
            {
                var fileName = this.ReadString();
                var fullName = this.ReadString();
                var attributes = this.ReadAttributes();

                files.Add(new SftpFile(fileName, fullName, attributes));
            }
            this.Files = files;
        }

        protected override void SaveData()
        {
            base.SaveData();
        }
    }
}
