using System;

namespace Renci.SshClient
{
    internal class MessageEventArgs<T> : EventArgs
    {
        public T Message { get; private set; }

        public MessageEventArgs(T message)
        {
            this.Message = message;
        }
    }
}
