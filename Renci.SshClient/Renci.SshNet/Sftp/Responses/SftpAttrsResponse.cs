using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpAttrsResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Attrs; }
        }

        public SftpFileAttributes Attributes { get; private set; }

        public SftpAttrsResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Attributes = this.ReadAttributes();
        }
    }
}
