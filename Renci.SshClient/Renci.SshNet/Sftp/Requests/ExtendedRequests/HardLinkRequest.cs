using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class HardLinkRequest : SftpExtendedRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Extended; }
        }
        
        public override string Name
        {
            get { return "hardlink@openssh.com"; }
        }

        public string OldPath { get; private set; }

        public string NewPath { get; private set; }

        public HardLinkRequest(uint protocolVersion, uint requestId, string oldPath, string newPath, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.OldPath = oldPath;
            this.NewPath = newPath;
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.OldPath);
            this.Write(this.NewPath);
        }
    }
}