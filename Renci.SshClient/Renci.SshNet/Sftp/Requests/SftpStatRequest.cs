using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpStatRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Stat; }
        }

        public string Path { get; private set; }

        public SftpStatRequest(uint protocolVersion, uint requestId, string path, Action<SftpAttrsResponse> attrsAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Path = path;
            this.SetAction(attrsAction);
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Path = this.ReadString();
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Path);
        }
    }
}
