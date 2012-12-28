using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class FStatVfsRequest : SftpExtendedRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Extended; }
        }

        public override string Name
        {
            get { return "fstatvfs@openssh.com"; }
        }

        public byte[] Handle { get; private set; }

        public FStatVfsRequest(uint protocolVersion, uint requestId, byte[] handle, Action<SftpExtendedReplyResponse> extendedAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Handle = handle;
            this.SetAction(extendedAction);
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
        }
    }
}
