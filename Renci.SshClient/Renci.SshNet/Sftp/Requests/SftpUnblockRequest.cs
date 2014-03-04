using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpUnblockRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Block; }
        }

        public byte[] Handle { get; private set; }

        public UInt64 Offset { get; private set; }

        public UInt64 Length { get; private set; }

        public SftpUnblockRequest(uint protocolVersion, uint requestId, byte[] handle, UInt64 offset, UInt64 length, UInt32 lockMask, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Handle = handle;
            this.Offset = offset;
            this.Length = length;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadBinaryString();
            this.Offset = this.ReadUInt64();
            this.Length = this.ReadUInt64();
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
