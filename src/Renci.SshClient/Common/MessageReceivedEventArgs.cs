using System;
using Renci.SshClient.Messages;

namespace Renci.SshClient.Common
{
    internal class MessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; private set; }

        public MessageReceivedEventArgs(Message message)
        {
            this.Message = message;
        }
    }
}
