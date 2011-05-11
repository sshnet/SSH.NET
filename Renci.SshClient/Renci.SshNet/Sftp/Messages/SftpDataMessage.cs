using System;
using System.Collections.Generic;
using System.Linq;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Sftp.Messages
{
    internal class SftpDataMessage : ChannelDataMessage
    {
        public SftpDataMessage(uint localChannelNumber, SftpMessage sftpMessage)
        {
            this.LocalChannelNumber = localChannelNumber;

            var messageData = sftpMessage.GetBytes();

            var data = new byte[4 + messageData.Length];

            ((uint)messageData.Length).GetBytes().CopyTo(data, 0);
            messageData.CopyTo(data, 4);
            this.Data = data;
        }
    }
}
