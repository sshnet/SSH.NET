using System;
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

        public Encoding Encoding { get; private set; }

        public SftpRealPathRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpNameResponse> nameAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            if (nameAction == null)
                throw new ArgumentNullException("nameAction");
            if (statusAction == null)
                throw new ArgumentNullException("statusAction");

            this.Path = path;
            this.Encoding = encoding;
            this.SetAction(nameAction);
            
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Path, this.Encoding);
        }
    }
}
