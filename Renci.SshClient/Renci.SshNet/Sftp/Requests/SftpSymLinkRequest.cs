using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpSymLinkRequest : SftpRequest
    {
#if TUNING
        private byte[] _newLinkPath;
        private byte[] _existingPath;
#endif

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.SymLink; }
        }

#if TUNING
        public string NewLinkPath
        {
            get { return Encoding.GetString(_newLinkPath, 0, _newLinkPath.Length); }
            private set { _newLinkPath = Encoding.GetBytes(value); }
        }
#else
        public string NewLinkPath { get; set; }
#endif

#if TUNING
        public string ExistingPath
        {
            get { return Encoding.GetString(_existingPath, 0, _existingPath.Length); }
            private set { _existingPath = Encoding.GetBytes(value); }
        }
#else
        public string ExistingPath { get; set; }
#endif

        public Encoding Encoding { get; set; }

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
                capacity += 4; // NewLinkPath length
                capacity += _newLinkPath.Length; // NewLinkPath
                capacity += 4; // ExistingPath length
                capacity += _existingPath.Length; // ExistingPath
                return capacity;
            }
        }
#endif

        public SftpSymLinkRequest(uint protocolVersion, uint requestId, string newLinkPath, string existingPath, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
#if TUNING
            this.Encoding = encoding;
#endif
            this.NewLinkPath = newLinkPath;
            this.ExistingPath = existingPath;
#if !TUNING
            this.Encoding = encoding;
#endif
        }

        protected override void LoadData()
        {
            base.LoadData();
#if TUNING
            _newLinkPath = ReadBinary();
            _existingPath = ReadBinary();
#else
            this.NewLinkPath = this.ReadString(this.Encoding);
            this.ExistingPath = this.ReadString(this.Encoding);
#endif
        }

        protected override void SaveData()
        {
            base.SaveData();
#if TUNING
            WriteBinaryString(_newLinkPath);
            WriteBinaryString(_existingPath);
#else
            this.Write(this.NewLinkPath, this.Encoding);
            this.Write(this.ExistingPath, this.Encoding);
#endif
        }
    }
}
