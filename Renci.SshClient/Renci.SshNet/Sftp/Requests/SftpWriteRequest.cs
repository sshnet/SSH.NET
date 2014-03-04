using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpWriteRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Write; }
        }

        public byte[] Handle { get; private set; }

        public UInt64 Offset { get; private set; }

        public byte[] Data { get; private set; }

        public SftpWriteRequest(uint protocolVersion, uint requestId, byte[] handle, UInt64 offset, byte[] data, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Handle = handle;
            this.Offset = offset;
            this.Data = data;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadBinaryString();
            this.Offset = this.ReadUInt64();
            this.Data = this.ReadBinaryString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
            this.Write(this.Offset);
            this.WriteBinaryString(this.Data);
        }
    }
}
