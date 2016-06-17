using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpLinkRequest : SftpRequest
    {
        private byte[] _newLinkPath;
        private byte[] _existingPath;

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Link; }
        }

        public string NewLinkPath
        {
            get { return Utf8.GetString(_newLinkPath, 0, _newLinkPath.Length); }
            private set { _newLinkPath = Utf8.GetBytes(value); }
        }

        public string ExistingPath
        {
            get { return Utf8.GetString(_existingPath, 0, _existingPath.Length); }
            private set { _existingPath = Utf8.GetBytes(value); }
        }

        public bool IsSymLink { get; private set; }

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
            NewLinkPath = newLinkPath;
            ExistingPath = existingPath;
            IsSymLink = isSymLink;
        }

        protected override void LoadData()
        {
            base.LoadData();
            _newLinkPath = ReadBinary();
            _existingPath = ReadBinary();
            IsSymLink = ReadBoolean();
        }

        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(_newLinkPath);
            WriteBinaryString(_existingPath);
            Write(IsSymLink);
        }
    }
}
