using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class PosixRenameRequest : SftpRequest
    {
        public const string NAME = "posix-rename@openssh.com";

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Extended; }
        }

        public string OldPath { get; private set; }

        public string NewPath { get; private set; }

        public PosixRenameRequest(uint protocolVersion, uint requestId, string oldPath, string newPath, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.OldPath = oldPath;
            this.NewPath = newPath;
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(PosixRenameRequest.NAME);
            this.Write(this.OldPath);
            this.Write(this.NewPath);
        }
    }
}