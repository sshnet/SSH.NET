using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpMkDirRequest : SftpRequest
    {
#if TUNING
        private byte[] _path;
        private byte[] _attributesBytes;
#endif

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.MkDir; }
        }

#if TUNING
        public string Path
        {
            get { return Encoding.GetString(_path); }
            private set { _path = Encoding.GetBytes(value); }
        }
#else
        public string Path { get; private set; }
#endif

        public Encoding Encoding { get; private set; }

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
                capacity += 4; // Path length
                capacity += _path.Length; // Path
                capacity += AttributesBytes.Length; // Attributes
                return capacity;
            }
        }
#endif

        public SftpMkDirRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpStatusResponse> statusAction)
#if TUNING
            : this(protocolVersion, requestId, path, encoding, SftpFileAttributes.Empty, statusAction)
#else
            : this(protocolVersion, requestId, path, encoding, null, statusAction)
#endif
        {
        }

        private SftpMkDirRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, SftpFileAttributes attributes, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Encoding = encoding;
            this.Path = path;
            Attributes = attributes;
        }

        protected override void LoadData()
        {
            base.LoadData();
#if TUNING
            _path = ReadBinary();
#else
            this.Path = this.ReadString(this.Encoding);
#endif
            this.Attributes = this.ReadAttributes();
        }

        protected override void SaveData()
        {
            base.SaveData();
#if TUNING
            WriteBinaryString(_path);
            Write(AttributesBytes);
#else
            this.Write(this.Path, this.Encoding);
            this.Write(this.Attributes);
#endif
        }
    }
}
