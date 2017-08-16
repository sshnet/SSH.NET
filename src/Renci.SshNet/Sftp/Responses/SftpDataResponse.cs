namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpDataResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Data; }
        }

        public byte[] Data { get; set; }

        public SftpDataResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        protected override void LoadData()
        {
            base.LoadData();
            
            Data = ReadBinary();
        }

        protected override void SaveData()
        {
            base.SaveData();

            WriteBinary(Data, 0, Data.Length);
        }
    }
}
