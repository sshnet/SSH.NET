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
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((uint)messageData.Count()).Reverse());
            data.AddRange(messageData);
            this.Data = data.GetSshString();

        }
    }
}
