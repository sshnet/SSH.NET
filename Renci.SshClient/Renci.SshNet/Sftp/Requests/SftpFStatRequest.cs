using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpFStatRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.FStat; }
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

        public SftpFStatRequest(uint protocolVersion, uint requestId, byte[] handle, Action<SftpAttrsResponse> attrsAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Handle = handle;
            this.SetAction(attrsAction);
        }

        protected override void LoadData()
        {
            base.LoadData();
#if TUNING
            this.Handle = this.ReadBinary();
#else
            this.Handle = this.ReadBinaryString();
#endif
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
        }
    }
}
