namespace Renci.SshNet.Sftp.Responses
{
    internal abstract class SftpResponse : SftpMessage
    {
        public uint ResponseId { get; set; }

        public uint ProtocolVersion { get; private set; }

        protected SftpResponse(uint protocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }

        protected override void LoadData()
        {
            base.LoadData();
            
            ResponseId = ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();

            Write(ResponseId);
        }
    }
}
