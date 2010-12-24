using System;

namespace Renci.SshClient
{
    public class MessageEventArgs<T> : EventArgs
    {
        public T Message { get; private set; }

        public MessageEventArgs(T message)
        {
            this.Message = message;
        }
    }
}
