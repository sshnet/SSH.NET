using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpExtendedReplyResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.ExtendedReply; }
        }

        public SftpExtendedReplyResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }
    }
}
