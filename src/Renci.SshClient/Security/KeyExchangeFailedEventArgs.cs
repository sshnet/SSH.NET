using System;

namespace Renci.SshClient.Security
{
    internal class KeyExchangeFailedEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public KeyExchangeFailedEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
