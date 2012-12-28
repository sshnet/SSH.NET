using System;
using System.Collections.Generic;
using System.Linq;
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

        public KeyValuePair<string, SftpFileAttributes>[] Files { get; private set; }

        public SftpNameResponse(uint protocolVersion)
            : base(protocolVersion)
        {
            this.Files = new KeyValuePair<string, SftpFileAttributes>[0];
        }

        protected override void LoadData()
        {
            base.LoadData();
            
            this.Count = this.ReadUInt32();
            this.Files = new KeyValuePair<string, SftpFileAttributes>[this.Count];
            
            for (int i = 0; i < this.Count; i++)
            {
                var fileName = this.ReadString();
                this.ReadString();   //  This field value has meaningless information
                var attributes = this.ReadAttributes();
                this.Files[i] = new KeyValuePair<string, SftpFileAttributes>(fileName, attributes);
            }
        }
    }
}
