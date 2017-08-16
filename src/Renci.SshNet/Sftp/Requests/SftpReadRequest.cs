using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpReadRequest : SftpRequest
    {
        private readonly Action<SftpDataResponse> _dataAction;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Read; }
        }

        public byte[] Handle { get; private set; }

        public ulong Offset { get; private set; }

        public uint Length { get; private set; }

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
                capacity += 4; // Length
                return capacity;
            }
        }

        public SftpReadRequest(uint protocolVersion, uint requestId, byte[] handle, UInt64 offset, UInt32 length, Action<SftpDataResponse> dataAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Handle = handle;
            Offset = offset;
            Length = length;
            _dataAction = dataAction;
        }

        protected override void LoadData()
        {
            base.LoadData();
            Handle = ReadBinary();
            Offset = ReadUInt64();
            Length = ReadUInt32();
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(Handle);
            Write(Offset);
            Write(Length);
        }

        public override void Complete(SftpResponse response)
        {
            var dataResponse = response as SftpDataResponse;
            if (dataResponse != null)
            {
                _dataAction(dataResponse);
            }
            else
            {
                base.Complete(response);
            }
        }
    }
}
