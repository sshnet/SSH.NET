using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpOpenRequest : SftpRequest
    {
        private byte[] _fileName;
        private byte[] _attributes;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Open; }
        }

        public string Filename
        {
            get { return Encoding.GetString(_fileName, 0, _fileName.Length); }
            private set { _fileName = Encoding.GetBytes(value); }
        }

        public Flags Flags { get; private set; }

        public SftpFileAttributes Attributes
        {
            get { return SftpFileAttributes.FromBytes(_attributes); }
            private set { _attributes = value.GetBytes(); }
        }

        public Encoding Encoding { get; private set; }

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

        public SftpOpenRequest(uint protocolVersion, uint requestId, string fileName, Encoding encoding, Flags flags, Action<SftpHandleResponse> handleAction, Action<SftpStatusResponse> statusAction)
            : this(protocolVersion, requestId, fileName, encoding, flags, SftpFileAttributes.Empty, handleAction, statusAction)
        {
        }

        private SftpOpenRequest(uint protocolVersion, uint requestId, string fileName, Encoding encoding, Flags flags, SftpFileAttributes attributes, Action<SftpHandleResponse> handleAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Encoding = encoding;
            Filename = fileName;
            Flags = flags;
            Attributes = attributes;

            SetAction(handleAction);
        }

        protected override void LoadData()
        {
            base.LoadData();
            throw new NotSupportedException();
        }

        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_fileName);
            Write((uint) Flags);
            Write(_attributes);
        }
    }
}
