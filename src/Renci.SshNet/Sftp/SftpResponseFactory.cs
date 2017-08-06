using System;
using System.Text;
using Renci.SshNet.Sftp.Responses;
using System.Globalization;

namespace Renci.SshNet.Sftp
{
    internal sealed class SftpResponseFactory : ISftpResponseFactory
    {
        public SftpMessage Create(uint protocolVersion, byte messageType, Encoding encoding)
        {
            var sftpMessageType = (SftpMessageTypes) messageType;

            SftpMessage message;

            switch (sftpMessageType)
            {
                case SftpMessageTypes.Version:
                    message = new SftpVersionResponse();
                    break;
                case SftpMessageTypes.Status:
                    message = new SftpStatusResponse(protocolVersion);
                    break;
                case SftpMessageTypes.Data:
                    message = new SftpDataResponse(protocolVersion);
                    break;
                case SftpMessageTypes.Handle:
                    message = new SftpHandleResponse(protocolVersion);
                    break;
                case SftpMessageTypes.Name:
                    message = new SftpNameResponse(protocolVersion, encoding);
                    break;
                case SftpMessageTypes.Attrs:
                    message = new SftpAttrsResponse(protocolVersion);
                    break;
                case SftpMessageTypes.ExtendedReply:
                    message = new SftpExtendedReplyResponse(protocolVersion);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Message type '{0}' is not supported.", sftpMessageType));
            }

            return message;
        }
    }
}
