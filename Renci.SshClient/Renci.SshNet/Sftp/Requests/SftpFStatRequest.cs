using System;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpFStatRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.FStat; }
        }

        public byte[] Handle { get; private set; }

        public SftpFStatRequest(uint protocolVersion, uint requestId, byte[] handle, Action<SftpAttrsResponse> attrsAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Handle = handle;
            this.SetAction(attrsAction);
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Handle = this.ReadBinaryString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.WriteBinaryString(this.Handle);
        }
    }
}
