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

#if true //old TUNING
        public int Length { get; private set; }

        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // Handle length
                capacity += Handle.Length; // Handle
                capacity += 8; // Offset length
                capacity += 4; // Data length
                capacity += Length; // Data
                return capacity;
            }
        }
#endif

        public SftpWriteRequest(uint protocolVersion,
                                uint requestId,
                                byte[] handle,
                                UInt64 offset,
                                byte[] data,
#if true //old TUNING
                                int length,
#endif
                                Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Handle = handle;
            this.Offset = offset;
            this.Data = data;
#if true //old TUNING
            this.Length = length;
#endif
        }

        protected override void LoadData()
        {
            base.LoadData();
#if true //old TUNING
            this.Handle = this.ReadBinary();
#else
            this.Handle = this.ReadBinaryString();
#endif
            this.Offset = this.ReadUInt64();
#if true //old TUNING
            this.Data = this.ReadBinary();
#else
            this.Data = this.ReadBinaryString();
#endif
#if true //old TUNING
            this.Length = this.Data.Length;
#endif
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
            this.Write(this.Offset);
#if true //old TUNING
            this.WriteBinary(this.Data, 0, Length);
#else
            this.WriteBinaryString(this.Data);
#endif
        }
    }
}
