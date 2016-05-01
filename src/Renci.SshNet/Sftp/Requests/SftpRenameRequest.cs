using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpRenameRequest : SftpRequest
    {
#if TUNING
        private byte[] _oldPath;
        private byte[] _newPath;
#endif

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Rename; }
        }

#if TUNING
        public string OldPath
        {
            get { return Encoding.GetString(_oldPath, 0, _oldPath.Length); }
            private set { _oldPath = Encoding.GetBytes(value); }
        }
#else
        public string OldPath { get; private set; }
#endif

#if TUNING
        public string NewPath
        {
            get { return Encoding.GetString(_newPath, 0, _newPath.Length); }
            private set { _newPath = Encoding.GetBytes(value); }
        }
#else
        public string NewPath { get; private set; }
#endif

        public Encoding Encoding { get; private set; }

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
                capacity += 4; // OldPath length
                capacity += _oldPath.Length; // OldPath
                capacity += 4; // NewPath length
                capacity += _newPath.Length; // NewPath
                return capacity;
            }
        }
#endif

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

#if TUNING
            _oldPath = ReadBinary();
            _newPath = ReadBinary();
#else
            OldPath = ReadString(Encoding);
            NewPath = ReadString(Encoding);
#endif

        }

        protected override void SaveData()
        {
            base.SaveData();

#if TUNING
            WriteBinaryString(_oldPath);
            WriteBinaryString(_newPath);
#else
            Write(OldPath, Encoding);
            Write(NewPath, Encoding);
#endif
        }
    }
}
