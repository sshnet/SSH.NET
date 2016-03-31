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

        public UInt64 Offset { get; private set; }

        public UInt64 Length { get; private set; }

#if true //old TUNING
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
#endif

        public SftpUnblockRequest(uint protocolVersion, uint requestId, byte[] handle, UInt64 offset, UInt64 length, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Handle = handle;
            this.Offset = offset;
            this.Length = length;
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
