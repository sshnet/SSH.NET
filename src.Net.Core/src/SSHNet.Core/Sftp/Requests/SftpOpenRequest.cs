using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpOpenRequest : SftpRequest
    {
#if true //old TUNING
        private byte[] _fileName;
        private byte[] _attributes;
#endif

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Open; }
        }

#if true //old TUNING
        public string Filename
        {
            get { return Encoding.GetString(_fileName, 0, _fileName.Length); }
            private set { _fileName = Encoding.GetBytes(value); }
        }
#else
        public string Filename { get; private set; }
#endif

        public Flags Flags { get; private set; }

#if true //old TUNING
        public SftpFileAttributes Attributes
        {
            get { return SftpFileAttributes.FromBytes(_attributes); }
            private set { _attributes = value.GetBytes(); }
        }
#else
        public SftpFileAttributes Attributes { get; private set; }
#endif

        public Encoding Encoding { get; private set; }

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
                capacity += 4; // FileName length
                capacity += _fileName.Length; // FileName
                capacity += 4; // Flags
                capacity += _attributes.Length; // Attributes
                return capacity;
            }
        }
#endif

        public SftpOpenRequest(uint protocolVersion, uint requestId, string fileName, Encoding encoding, Flags flags, Action<SftpHandleResponse> handleAction, Action<SftpStatusResponse> statusAction)
            : this(protocolVersion, requestId, fileName, encoding, flags, SftpFileAttributes.Empty, handleAction, statusAction)
        {
        }

        private SftpOpenRequest(uint protocolVersion, uint requestId, string fileName, Encoding encoding, Flags flags, SftpFileAttributes attributes, Action<SftpHandleResponse> handleAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Encoding = encoding;
            this.Filename = fileName;
            this.Flags = flags;
            this.Attributes = attributes;

            this.SetAction(handleAction);
        }

        protected override void LoadData()
        {
            base.LoadData();
            throw new NotSupportedException();
        }

        protected override void SaveData()
        {
            base.SaveData();

#if true //old TUNING
            WriteBinaryString(_fileName);
#else
            this.Write(this.Filename, this.Encoding);
#endif
            this.Write((uint)this.Flags);
#if true //old TUNING
            this.Write(_attributes);
#else
            this.Write(this.Attributes);
#endif
        }
    }
}
