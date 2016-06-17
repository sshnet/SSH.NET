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

        public ulong Offset { get; private set; }

        public byte[] Data { get; private set; }

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

        public SftpWriteRequest(uint protocolVersion,
                                uint requestId,
                                byte[] handle,
                                ulong offset,
                                byte[] data,
                                int length,
                                Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Handle = handle;
            Offset = offset;
            Data = data;
            Length = length;
        }

        protected override void LoadData()
        {
            base.LoadData();
            Handle = ReadBinary();
            Offset = ReadUInt64();
            Data = ReadBinary();
            Length = Data.Length;
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(Handle);
            Write(Offset);
            WriteBinary(Data, 0, Length);
        }
    }
}
