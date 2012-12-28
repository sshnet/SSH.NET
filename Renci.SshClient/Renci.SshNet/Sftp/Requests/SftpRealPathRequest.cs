using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpRealPathRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.RealPath; }
        }

        public string Path { get; private set; }

        public SftpRealPathRequest(uint protocolVersion, uint requestId, string path, Action<SftpNameResponse> nameAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            if (nameAction == null)
                throw new ArgumentNullException("name");

            if (statusAction == null)
                throw new ArgumentNullException("status");

            this.Path = path;
            this.SetAction(nameAction);
            
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Path);
        }
    }
}
