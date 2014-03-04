using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpRmDirRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.RmDir; }
        }

        public string Path { get; private set; }

        public Encoding Encoding { get; private set; }

        public SftpRmDirRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Path = path;
            this.Encoding = encoding;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Path = this.ReadString(this.Encoding);
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Path, this.Encoding);
        }
    }
}
