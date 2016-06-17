using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpMkDirRequest : SftpRequest
    {
        private byte[] _path;
        private byte[] _attributesBytes;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.MkDir; }
        }

        public string Path
        {
            get { return Encoding.GetString(_path, 0, _path.Length); }
            private set { _path = Encoding.GetBytes(value); }
        }

        public Encoding Encoding { get; private set; }

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

        public SftpMkDirRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : this(protocolVersion, requestId, path, encoding, SftpFileAttributes.Empty, statusAction)
        {
        }

        private SftpMkDirRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, SftpFileAttributes attributes, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Encoding = encoding;
            Path = path;
            Attributes = attributes;
        }

        protected override void LoadData()
        {
            base.LoadData();
            _path = ReadBinary();
            Attributes = ReadAttributes();
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(_path);
            Write(AttributesBytes);
        }
    }
}
