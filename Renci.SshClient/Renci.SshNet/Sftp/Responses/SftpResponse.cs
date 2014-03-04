using System;

namespace Renci.SshNet.Sftp.Responses
{
    internal abstract class SftpResponse : SftpMessage
    {
        public uint ResponseId { get; private set; }

        public uint ProtocolVersion { get; private set; }

        public SftpResponse(uint protocolVersion)
        {
            this.ProtocolVersion = protocolVersion;
        }

        protected override void LoadData()
        {
            base.LoadData();
            
            this.ResponseId = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            throw new InvalidOperationException("Response cannot be saved.");
        }
    }
}
