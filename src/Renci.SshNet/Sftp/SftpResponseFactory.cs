using System;
using System.Globalization;
using System.Text;

using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// A factory for creating SFTP response messages.
    /// </summary>
    internal sealed class SftpResponseFactory : ISftpResponseFactory
    {
        /// <summary>
        /// Creates a SFTP response message for the specified protocol version and message type, and
        /// with the specified <see cref="Encoding"/>.
        /// </summary>
        /// <param name="protocolVersion">The protocol version.</param>
        /// <param name="messageType">The message type.</param>
        /// <param name="encoding">The <see cref="Encoding"/>.</param>
        /// <returns>
        /// A <see cref="SftpMessage"/>.
        /// </returns>
        public SftpMessage Create(uint protocolVersion, byte messageType, Encoding encoding)
        {
            var sftpMessageType = (SftpMessageTypes) messageType;

            SftpMessage message;

#pragma warning disable IDE0010 // Add missing cases
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
#pragma warning restore IDE0010 // Add missing cases

            return message;
        }
    }
}
