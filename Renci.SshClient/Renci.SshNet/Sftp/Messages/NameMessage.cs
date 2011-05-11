using System.Collections.Generic;

namespace Renci.SshNet.Sftp.Messages
{
    internal class NameMessage : SftpRequestMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Name; }
        }

        public uint Count { get; private set; }

        public IDictionary<string, SftpFileAttributes> Files { get; private set; }

        public NameMessage()
        {
            this.Files = new Dictionary<string, SftpFileAttributes>();
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Count = this.ReadUInt32();
            for (int i = 0; i < this.Count; i++)
            {
                var fileName = this.ReadString();
                this.ReadString();   //  This field value has meaningless information
                var attributes = this.ReadAttributes();

                this.Files.Add(fileName, attributes);
            }
        }

        protected override void SaveData()
        {
            base.SaveData();
        }
    }
}
