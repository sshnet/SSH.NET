using System;
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

        public Encoding Encoding { get; private set; }

        public SftpRemoveRequest(uint protocolVersion, uint requestId, string filename, Encoding encoding, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Filename = filename;
            this.Encoding = encoding;
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Filename = this.ReadString(this.Encoding);
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Filename, this.Encoding);
        }
    }
}
