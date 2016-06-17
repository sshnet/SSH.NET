namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpAttrsResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Attrs; }
        }

        public SftpFileAttributes Attributes { get; private set; }

        public SftpAttrsResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        protected override void LoadData()
        {
            base.LoadData();
            Attributes = ReadAttributes();
        }
    }
}
