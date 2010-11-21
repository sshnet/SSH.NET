using System;

namespace Renci.SshClient.Common
{
    internal class ChannelEventArgs : EventArgs
    {
        public uint ChannelNumber { get; private set; }

        public ChannelEventArgs(uint channelNumber)
        {
            this.ChannelNumber = channelNumber;
        }
    }
}
