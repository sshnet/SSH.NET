using System.Collections.Generic;

namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpVersionResponse : SftpMessage
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Version; }
        }

        public uint Version { get; set; }

        public IDictionary<string, string> Extentions { get; set; }

        protected override void LoadData()
        {
            base.LoadData();
            Version = ReadUInt32();
            Extentions = ReadExtensionPair();
        }

        protected override void SaveData()
        {
            base.SaveData();

            Write(Version);
            if (Extentions != null)
                Write(Extentions);
        }
    }
}
