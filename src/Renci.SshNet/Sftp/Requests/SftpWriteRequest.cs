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

        /// <summary>
        /// Gets the zero-based offset (in bytes) relative to the beginning of the file that the write
        /// must start at.
        /// </summary>
        /// <value>
        /// The zero-based offset (in bytes) relative to the beginning of the file that the write must
        /// start at.
        /// </value>
        public ulong ServerFileOffset { get; private set; }

        /// <summary>
        /// Gets the buffer holding the data to write.
        /// </summary>
        /// <value>
        /// The buffer holding the data to write.
        /// </value>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Gets the zero-based offset in <see cref="Data" /> at which to begin taking bytes to
        /// write.
        /// </summary>
        /// <value>
        /// The zero-based offset in <see cref="Data" /> at which to begin taking bytes to write.
        /// </value>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets the length (in bytes) of the data to write.
        /// </summary>
        /// <value>
        /// The length (in bytes) of the data to write.
        /// </value>
        public int Length { get; private set; }

        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // Handle length
                capacity += Handle.Length; // Handle
                capacity += 8; // ServerFileOffset length
                capacity += 4; // Data length
                capacity += Length; // Data
                return capacity;
            }
        }

        public SftpWriteRequest(uint protocolVersion,
                                uint requestId,
                                byte[] handle,
                                ulong serverFileOffset,
                                byte[] data,
                                int offset,
                                int length,
                                Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Handle = handle;
            ServerFileOffset = serverFileOffset;
            Data = data;
            Offset = offset;
            Length = length;
        }

        protected override void LoadData()
        {
            base.LoadData();
            Handle = ReadBinary();
            ServerFileOffset = ReadUInt64();
            Data = ReadBinary();
            Offset = 0;
            Length = Data.Length;
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(Handle);
            Write(ServerFileOffset);
            WriteBinary(Data, Offset, Length);
        }
    }
}
