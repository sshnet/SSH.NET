using System;

namespace Renci.SshClient.Channels
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
