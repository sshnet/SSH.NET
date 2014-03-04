using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpLinkRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Link; }
        }

        public string NewLinkPath { get; private set; }

        public string ExistingPath { get; private set; }

        public bool IsSymLink { get; private set; }

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
            this.NewLinkPath = this.ReadString();
            this.ExistingPath = this.ReadString();
            this.IsSymLink = this.ReadBoolean();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.NewLinkPath);
            this.Write(this.ExistingPath);
            this.Write(this.IsSymLink);
        }
    }
}
