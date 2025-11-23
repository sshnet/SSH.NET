using System.Collections.Generic;

namespace Renci.SshNet.Sftp.Responses
{
    internal sealed class SftpVersionResponse : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Version; }
        }

        public uint Version { get; set; }

        public IDictionary<string, string> Extensions { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            Version = ReadUInt32();
            Extensions = ReadExtensionPair();
        }

        protected override void SaveData()
        {
            base.SaveData();

            Write(Version);

            if (Extensions != null)
            {
                Write(Extensions);
            }
        }
    }
}
