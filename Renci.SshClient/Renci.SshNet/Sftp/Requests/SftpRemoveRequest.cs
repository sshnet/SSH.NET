using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpRemoveRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Remove; }
        }

        public string Filename { get; private set; }

        public SftpRemoveRequest(uint protocolVersion, uint requestId, string filename, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Filename = filename;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Filename = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Filename);
        }
    }
}
