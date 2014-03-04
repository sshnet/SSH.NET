using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpOpenRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Open; }
        }

        public string Filename { get; private set; }

        public Flags Flags { get; private set; }

        public SftpFileAttributes Attributes { get; private set; }

        public Encoding Encoding { get; private set; }

        public SftpOpenRequest(uint protocolVersion, uint requestId, string fileName, Encoding encoding, Flags flags, Action<SftpHandleResponse> handleAction, Action<SftpStatusResponse> statusAction)
            : this(protocolVersion, requestId, fileName, encoding, flags, new SftpFileAttributes(), handleAction, statusAction)
        {
        }

        public SftpOpenRequest(uint protocolVersion, uint requestId, string fileName, Encoding encoding, Flags flags, SftpFileAttributes attributes, Action<SftpHandleResponse> handleAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Filename = fileName;
            this.Flags = flags;
            this.Attributes = attributes;
            this.Encoding = encoding;

            this.SetAction(handleAction);
        }

        protected override void LoadData()
        {
            base.LoadData();
            throw new NotSupportedException();
        }

        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.Filename, this.Encoding);
            this.Write((uint)this.Flags);
            this.Write(this.Attributes);
        }
    }
}
