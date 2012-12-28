using System;
using System.Collections.Generic;
using System.Linq;
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

        public SftpOpenRequest(uint requestId, string fileName, Flags flags, Action<SftpHandleResponse> handleAction, Action<SftpStatusResponse> statusAction)
            : this(requestId, fileName, flags, new SftpFileAttributes(), handleAction, statusAction)
        {
        }

        public SftpOpenRequest(uint requestId, string fileName, Flags flags, SftpFileAttributes attributes, Action<SftpHandleResponse> handleAction, Action<SftpStatusResponse> statusAction)
            : base(requestId, statusAction)
        {
            this.Filename = fileName;
            this.Flags = flags;
            this.Attributes = attributes;

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

            this.Write(this.Filename);
            this.Write((uint)this.Flags);
            this.Write(this.Attributes);
        }
    }
}
