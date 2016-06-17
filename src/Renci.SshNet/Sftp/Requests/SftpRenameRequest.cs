using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpRenameRequest : SftpRequest
    {
        private byte[] _oldPath;
        private byte[] _newPath;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Rename; }
        }

        public string OldPath
        {
            get { return Encoding.GetString(_oldPath, 0, _oldPath.Length); }
            private set { _oldPath = Encoding.GetBytes(value); }
        }

        public string NewPath
        {
            get { return Encoding.GetString(_newPath, 0, _newPath.Length); }
            private set { _newPath = Encoding.GetBytes(value); }
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
                capacity += 4; // OldPath length
                capacity += _oldPath.Length; // OldPath
                capacity += 4; // NewPath length
                capacity += _newPath.Length; // NewPath
                return capacity;
            }
        }

        public SftpRenameRequest(uint protocolVersion, uint requestId, string oldPath, string newPath, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            Encoding = encoding;
            OldPath = oldPath;
            NewPath = newPath;
        }

        protected override void LoadData()
        {
            base.LoadData();

            _oldPath = ReadBinary();
            _newPath = ReadBinary();
        }

        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_oldPath);
            WriteBinaryString(_newPath);
        }
    }
}
