namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpDataResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Data; }
        }

        public byte[] Data { get; private set; }

        public bool IsEof { get; private set; }

        public SftpDataResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        protected override void LoadData()
        {
            base.LoadData();
            
#if TUNING
            Data = ReadBinary();
#else
            Data = ReadBinaryString();
#endif

            if (!IsEndOfData)
            {
                IsEof = ReadBoolean();
            }
        }
    }
}
