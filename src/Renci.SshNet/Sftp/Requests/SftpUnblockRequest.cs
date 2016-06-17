using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpUnblockRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Unblock; }
        }

        public byte[] Handle { get; private set; }

        public ulong Offset { get; private set; }

        public ulong Length { get; private set; }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // Handle length
                capacity += Handle.Length; // Handle
                capacity += 8; // Offset
                capacity += 8; // Length
                return capacity;
            }
        }

        public SftpUnblockRequest(uint protocolVersion, uint requestId, byte[] handle, UInt64 offset, UInt64 length, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Handle = handle;
            Offset = offset;
            Length = length;
        }

        protected override void LoadData()
        {
            base.LoadData();
            Handle = ReadBinary();
            Offset = ReadUInt64();
            Length = ReadUInt64();
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(Handle);
            Write(Offset);
            Write(Length);
        }
    }
}
