using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpNameResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Name; }
        }

        public uint Count { get; private set; }

        public Encoding Encoding { get; private set; }

        public KeyValuePair<string, SftpFileAttributes>[] Files { get; private set; }

        public SftpNameResponse(uint protocolVersion, Encoding encoding)
            : base(protocolVersion)
        {
            this.Files = new KeyValuePair<string, SftpFileAttributes>[0];
            this.Encoding = encoding;
        }

        protected override void LoadData()
        {
            base.LoadData();
            
            this.Count = this.ReadUInt32();
            this.Files = new KeyValuePair<string, SftpFileAttributes>[this.Count];
            
            for (int i = 0; i < this.Count; i++)
            {
                var fileName = this.ReadString(this.Encoding);
                this.ReadString();   //  This field value has meaningless information
                var attributes = this.ReadAttributes();
                this.Files[i] = new KeyValuePair<string, SftpFileAttributes>(fileName, attributes);
            }
        }
    }
}
