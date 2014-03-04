using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpRenameRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Rename; }
        }

        public string OldPath { get; private set; }

        public string NewPath { get; private set; }

        public Encoding Encoding { get; private set; }

        public SftpRenameRequest(uint protocolVersion, uint requestId, string oldPath, string newPath, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.OldPath = oldPath;
            this.NewPath = newPath;
            this.Encoding = encoding;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.OldPath = this.ReadString(this.Encoding);
            this.NewPath = this.ReadString(this.Encoding);
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.OldPath, this.Encoding);
            this.Write(this.NewPath, this.Encoding);
        }
    }
}
