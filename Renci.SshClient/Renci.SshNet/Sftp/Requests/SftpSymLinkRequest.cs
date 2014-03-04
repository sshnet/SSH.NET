using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpSymLinkRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.SymLink; }
        }

        public string NewLinkPath { get; set; }

        public string ExistingPath { get; set; }

        public Encoding Encoding { get; set; }

        public SftpSymLinkRequest(uint protocolVersion, uint requestId, string newLinkPath, string existingPath, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.NewLinkPath = newLinkPath;
            this.ExistingPath = existingPath;
            this.Encoding = encoding;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.NewLinkPath = this.ReadString(this.Encoding);
            this.ExistingPath = this.ReadString(this.Encoding);
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.NewLinkPath, this.Encoding);
            this.Write(this.ExistingPath, this.Encoding);
        }
    }
}
