using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpLinkRequest : SftpRequest
    {
#if TUNING
        private byte[] _newLinkPath;
        private byte[] _existingPath;
#endif

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Link; }
        }

#if TUNING
        public string NewLinkPath
        {
            get { return Utf8.GetString(_newLinkPath); }
            private set { _newLinkPath = Utf8.GetBytes(value); }
        }
#else
        public string NewLinkPath { get; private set; }
#endif

#if TUNING
        public string ExistingPath
        {
            get { return Utf8.GetString(_existingPath); }
            private set { _existingPath = Utf8.GetBytes(value); }
        }
#else
        public string ExistingPath { get; private set; }
#endif

        public bool IsSymLink { get; private set; }

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
                capacity += NewLinkPath.Length; // NewLinkPath
                capacity += 4; // ExistingPath length
                capacity += ExistingPath.Length; // ExistingPath
                capacity += 1; // IsSymLink
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpLinkRequest" /> class.
        /// </summary>
        /// <param name="protocolVersion">The protocol version.</param>
        /// <param name="requestId">The request id.</param>
        /// <param name="newLinkPath">Specifies the path name of the new link to create.</param>
        /// <param name="existingPath">Specifies the path of a target object to which the newly created link will refer.  In the case of a symbolic link, this path may not exist.</param>
        /// <param name="isSymLink">if set to <c>false</c> the link should be a hard link, or a second directory entry referring to the same file or directory object.</param>
        /// <param name="statusAction">The status action.</param>
        public SftpLinkRequest(uint protocolVersion, uint requestId, string newLinkPath, string existingPath, bool isSymLink, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.NewLinkPath = newLinkPath;
            this.ExistingPath = existingPath;
            this.IsSymLink = isSymLink;
        }

        protected override void LoadData()
        {
            base.LoadData();
#if TUNING
            _newLinkPath = ReadBinary();
            _existingPath = ReadBinary();
#else
            this.NewLinkPath = this.ReadString();
            this.ExistingPath = this.ReadString();
#endif
            this.IsSymLink = this.ReadBoolean();
        }

        protected override void SaveData()
        {
            base.SaveData();
#if TUNING
            WriteBinaryString(_newLinkPath);
            WriteBinaryString(_existingPath);
#else
            this.Write(this.NewLinkPath);
            this.Write(this.ExistingPath);
#endif
            this.Write(this.IsSymLink);
        }
    }
}
