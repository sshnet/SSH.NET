using System;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient
{
    internal class ChannelOpeningEventArgs : EventArgs
    {
        public ChannelOpenMessage Message { get; set; }

    }
}
