using System;
using Renci.SshNet.Sftp.Responses;
using System.Text;

namespace Renci.SshNet.Sftp.Requests
{
    internal class PosixRenameRequest : SftpExtendedRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Extended; }
        }
        
        public override string Name
        {
            get { return "posix-rename@openssh.com"; }
        }

        public string OldPath { get; private set; }

        public string NewPath { get; private set; }

        public Encoding Encoding { get; private set; }

        public PosixRenameRequest(uint protocolVersion, uint requestId, string oldPath, string newPath, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.OldPath = oldPath;
            this.NewPath = newPath;
            this.Encoding = encoding;
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.OldPath, this.Encoding);
            this.Write(this.NewPath, this.Encoding);
        }
    }
}