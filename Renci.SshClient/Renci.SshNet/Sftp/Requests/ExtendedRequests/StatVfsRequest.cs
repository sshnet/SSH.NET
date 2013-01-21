using System;
using Renci.SshNet.Sftp.Responses;
using System.Text;

namespace Renci.SshNet.Sftp.Requests
{
    internal class StatVfsRequest : SftpExtendedRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Extended; }
        }

        public override string Name
        {
            get { return "statvfs@openssh.com"; }
        }

        public string Path { get; private set; }

        public Encoding Encoding { get; private set; }

        public StatVfsRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpExtendedReplyResponse> extendedAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Path = path;
            this.Encoding = encoding;
            this.SetAction(extendedAction);
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Path, this.Encoding);
        }
    }
}
