using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpReadRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Read; }
        }

        public byte[] Handle { get; private set; }

        public UInt64 Offset { get; private set; }

        public UInt32 Length { get; private set; }

        public SftpReadRequest(uint protocolVersion, uint requestId, byte[] handle, UInt64 offset, UInt32 length, Action<SftpDataResponse> dataAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Handle = handle;
            this.Offset = offset;
            this.Length = length;
            this.SetAction(dataAction);
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadBinaryString();
            this.Offset = this.ReadUInt64();
            this.Length = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
            this.Write(this.Offset);
            this.Write(this.Length);
        }
    }
}
