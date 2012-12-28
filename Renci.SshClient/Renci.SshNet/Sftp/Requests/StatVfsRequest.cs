using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class StatVfsRequest : SftpRequest
    {
        public const string NAME = "statvfs@openssh.com";

        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Extended; }
        }

        public string Path { get; private set; }

        public StatVfsRequest(uint protocolVersion, uint requestId, string path, Action<SftpExtendedReplyResponse> extendedAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Path = path;
            this.SetAction(extendedAction);
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(StatVfsRequest.NAME);
            this.Write(this.Path);
        }
    }
}
