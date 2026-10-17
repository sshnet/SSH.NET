using System;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Sftp.Responses
{
    internal sealed class SftpNameResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Name; }
        }

        public uint Count { get; private set; }

        public Encoding Encoding { get; private set; }

        public KeyValuePair<string, SftpFileAttributes>[] Files { get; set; }

        public SftpNameResponse(uint protocolVersion, Encoding encoding)
            : base(protocolVersion)
        {
            Files = Array.Empty<KeyValuePair<string, SftpFileAttributes>>();
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
                string longName = null;
                if (SupportsLongName(ProtocolVersion))
                {
                    longName = ReadString(Encoding);
                }

                var attributes = ReadAttributes();
                attributes.LongName = longName;
                Files[i] = new KeyValuePair<string, SftpFileAttributes>(fileName, attributes);
            }
        }

        protected override void SaveData()
        {
            base.SaveData();

            Write((uint)Files.Length); // count

            for (var i = 0; i < Files.Length; i++)
            {
                var file = Files[i];

                Write(file.Key, Encoding); // filename

                if (SupportsLongName(ProtocolVersion))
                {
                    Write(file.Value.LongName ?? string.Empty, Encoding); // longname
                }

                Write(file.Value.GetBytes()); // attrs
            }
        }

        private static bool SupportsLongName(uint protocolVersion)
        {
            return protocolVersion <= 3U;
        }
    }
}
