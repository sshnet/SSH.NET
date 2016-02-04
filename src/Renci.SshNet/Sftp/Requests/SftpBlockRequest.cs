using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpBlockRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Block; }
        }

        public byte[] Handle { get; private set; }

        public UInt64 Offset { get; private set; }

        public UInt64 Length { get; private set; }

        public UInt32 LockMask { get; private set; }

#if TUNING
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
                capacity += 4; // LockMask
                return capacity;
            }
        }
#endif

        public SftpBlockRequest(uint protocolVersion, uint requestId, byte[] handle, UInt64 offset, UInt64 length, UInt32 lockMask, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Handle = handle;
            this.Offset = offset;
            this.Length = length;
            this.LockMask = lockMask;
        }

        protected override void LoadData()
        {
            base.LoadData();
#if TUNING
            this.Handle = this.ReadBinary();
#else
            this.Handle = this.ReadBinaryString();
#endif
            this.Offset = this.ReadUInt64();
            this.Length = this.ReadUInt64();
            this.LockMask = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
            this.Write(this.Offset);
            this.Write(this.Length);
            this.Write(this.LockMask);
        }
    }
}
