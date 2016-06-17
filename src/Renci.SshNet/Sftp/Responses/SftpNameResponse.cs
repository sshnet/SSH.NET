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
            Files = new KeyValuePair<string, SftpFileAttributes>[0];
            Encoding = encoding;
        }

        protected override void LoadData()
        {
            base.LoadData();
            
            Count = ReadUInt32();
            Files = new KeyValuePair<string, SftpFileAttributes>[Count];
            
            for (var i = 0; i < Count; i++)
            {
                var fileName = ReadString(Encoding);
                ReadString(Encoding);   //  skip longname
                var attributes = ReadAttributes();
                Files[i] = new KeyValuePair<string, SftpFileAttributes>(fileName, attributes);
            }
        }
    }
}
