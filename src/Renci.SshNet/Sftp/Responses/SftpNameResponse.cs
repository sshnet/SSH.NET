using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Renci.SshNet.Common;

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

            // Each entry is at least two uints in size.
            if (Count > (DataStream.Length - DataStream.Position) / 8)
            {
                throw new SshException(string.Format(CultureInfo.InvariantCulture, "Invalid SSH_FXP_NAME count: {0}.", Count));
            }

            Files = new KeyValuePair<string, SftpFileAttributes>[Count];

            for (var i = 0; i < Count; i++)
            {
                var fileName = ReadString(Encoding);
                if (SupportsLongName(ProtocolVersion))
                {
                    _ = ReadString(Encoding); // skip longname
                }

                Files[i] = new KeyValuePair<string, SftpFileAttributes>(fileName, ReadAttributes());
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
                    Write(0U); // longname
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
