using System;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Channels
{
    internal class ChannelRequestEventArgs : EventArgs
    {
        public RequestInfo Info { get; private set; }

        public ChannelRequestEventArgs(RequestInfo info)
        {
            this.Info = info;
        }
    }
}
