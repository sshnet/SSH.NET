using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpCloseRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Close; }
        }

        public byte[] Handle { get; private set; }

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
                return capacity;
            }
        }
#endif

        public SftpCloseRequest(uint protocolVersion, uint requestId, byte[] handle, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Handle = handle;
        }

        protected override void LoadData()
        {
            base.LoadData();
#if TUNING
            Handle = ReadBinary();
#else
            Handle = this.ReadBinaryString();
#endif
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(Handle);
        }
    }
}
