using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpSymLinkRequest : SftpRequest
    {
        private byte[] _newLinkPath;
        private byte[] _existingPath;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.SymLink; }
        }

        public string NewLinkPath
        {
            get { return Encoding.GetString(_newLinkPath, 0, _newLinkPath.Length); }
            private set { _newLinkPath = Encoding.GetBytes(value); }
        }

        public string ExistingPath
        {
            get { return Encoding.GetString(_existingPath, 0, _existingPath.Length); }
            private set { _existingPath = Encoding.GetBytes(value); }
        }

        public Encoding Encoding { get; set; }

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
                capacity += 4; // NewLinkPath length
                capacity += _newLinkPath.Length; // NewLinkPath
                capacity += 4; // ExistingPath length
                capacity += _existingPath.Length; // ExistingPath
                return capacity;
            }
        }

        public SftpSymLinkRequest(uint protocolVersion, uint requestId, string newLinkPath, string existingPath, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Encoding = encoding;
            NewLinkPath = newLinkPath;
            ExistingPath = existingPath;
        }

        protected override void LoadData()
        {
            base.LoadData();
            _newLinkPath = ReadBinary();
            _existingPath = ReadBinary();
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(_newLinkPath);
            WriteBinaryString(_existingPath);
        }
    }
}
