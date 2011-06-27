using System;
using System.Collections.Generic;
using System.Linq;
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

        public SftpSymLinkRequest(uint requestId, string newLinkPath, string existingPath, Action<SftpStatusResponse> statusAction)
            : base(requestId, statusAction)
        {
            this.NewLinkPath = newLinkPath;
            this.ExistingPath = existingPath;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.NewLinkPath = this.ReadString();
            this.ExistingPath = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.NewLinkPath, Encoding.UTF8);
            this.Write(this.ExistingPath, Encoding.UTF8);
        }
    }
}
