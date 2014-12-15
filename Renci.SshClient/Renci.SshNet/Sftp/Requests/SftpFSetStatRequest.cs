using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpFSetStatRequest : SftpRequest
    {
#if TUNING
        private byte[] _attributesBytes;
#endif

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.FSetStat; }
        }

        public byte[] Handle { get; private set; }

#if TUNING
        private SftpFileAttributes Attributes { get; set; }

        private byte[] AttributesBytes
        {
            get
            {
                if (_attributesBytes == null)
                {
                    _attributesBytes = Attributes.GetBytes();
                }
                return _attributesBytes;
            }
        }
#else
        public SftpFileAttributes Attributes { get; private set; }
#endif

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
                capacity += AttributesBytes.Length; // Attributes
                return capacity;
            }
        }
#endif

        public SftpFSetStatRequest(uint protocolVersion, uint requestId, byte[] handle, SftpFileAttributes attributes, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Handle = handle;
            this.Attributes = attributes;
        }

        protected override void LoadData()
        {
            base.LoadData();
#if TUNING
            this.Handle = this.ReadBinary();
#else
            this.Handle = this.ReadBinaryString();
#endif
            this.Attributes = this.ReadAttributes();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
#if TUNING
            Write(AttributesBytes);
#else
            this.Write(this.Attributes);
#endif
        }
    }
}
