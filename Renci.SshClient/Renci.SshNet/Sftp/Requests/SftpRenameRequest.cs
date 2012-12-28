using System;
using System.Collections.Generic;
using System.Linq;
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

        public SftpRenameRequest(uint protocolVersion, uint requestId, string oldPath, string newPath, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.OldPath = oldPath;
            this.NewPath = newPath;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.OldPath = this.ReadString();
            this.NewPath = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.OldPath);
            this.Write(this.NewPath);
        }
    }
}
