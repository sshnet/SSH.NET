using System;
using Renci.SshClient.Messages;

namespace Renci.SshClient.Algorithms
{
    internal class KeyExchangeSendMessageEventArgs : EventArgs
    {
        public KeyExchangeSendMessageEventArgs(Message message)
        {
            this.Message = message;
        }

        public Message Message { get; private set; }
    }
}
