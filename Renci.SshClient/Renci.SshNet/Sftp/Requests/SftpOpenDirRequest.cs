using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpOpenDirRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.OpenDir; }
        }

        public string Path { get; private set; }

        public SftpOpenDirRequest(uint protocolVersion, uint requestId, string path, Action<SftpHandleResponse> handleAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Path = path;
            this.SetAction(handleAction);
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
