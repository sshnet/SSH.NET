using System;

namespace Renci.SshNet.Sftp.Responses
{
    internal sealed class SftpDataResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Data; }
        }

        public ArraySegment<byte> Data { get; set; }

        public SftpDataResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        protected override void LoadData()
        {
            base.LoadData();

            Data = ReadBinarySegment();
        }

        protected override void SaveData()
        {
            base.SaveData();

            WriteBinary(Data.Array, Data.Offset, Data.Count);
        }
    }
}
