namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpHandleResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Handle; }
        }

        public byte[] Handle { get; set; }

        public SftpHandleResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        protected override void LoadData()
        {
            base.LoadData();
            
            Handle = ReadBinary();
        }

        protected override void SaveData()
        {
            base.SaveData();

            WriteBinary(Handle, 0, Handle.Length);
        }
    }
}
